using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Logistics.ValueObjects;
using MsLogistic.Domain.Shared.ValueObjects;
using MsLogistic.Infrastructure.External.GoogleMaps;
using MsLogistic.UnitTest.Common.Http;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.External.GoogleMaps;

public class GoogleMapsRouteCalculatorTest {
	private readonly FakeHttpMessageHandler _handler;
	private readonly GoogleMapsRouteCalculator _calculator;

	public GoogleMapsRouteCalculatorTest() {
		_handler = new FakeHttpMessageHandler();
		var httpClient = new HttpClient(_handler);
		IOptions<GoogleMapsOptions> options = Options.Create(new GoogleMapsOptions {
			BaseUrl = "https://maps.googleapis.com/maps/api",
			ApiKey = "test-api-key"
		});
		ILogger<GoogleMapsRouteCalculator> logger = new Mock<ILogger<GoogleMapsRouteCalculator>>().Object;
		_calculator = new GoogleMapsRouteCalculator(httpClient, options, logger);
	}

	private static GeoPointValue CreateOrigin() => GeoPointValue.Create(-16.50, -68.15);

	private static List<Waypoint> CreateWaypoints(int count) {
		var waypoints = new List<Waypoint>();
		for (int i = 0; i < count; i++) {
			waypoints.Add(new Waypoint(
				Guid.NewGuid(),
				GeoPointValue.Create(-16.50 + i * 0.01, -68.15 + i * 0.01)
			));
		}

		return waypoints;
	}

	private static string SerializeResponse(GoogleMapsDirectionsResponse response) {
		return JsonSerializer.Serialize(response, new JsonSerializerOptions {
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		});
	}

	[Fact]
	public async Task CalculateOrderAsync_WithLessThanTwoWaypoints_ShouldReturnFailureWithoutHttpCall() {
		// Arrange
		GeoPointValue origin = CreateOrigin();
		List<Waypoint> waypoints = CreateWaypoints(1);

		// Act
		Result<OrderedRoute> result = await _calculator.CalculateOrderAsync(
			origin, waypoints, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().NotBeNull();
		_handler.RequestCount.Should().Be(0);
	}

	[Fact]
	public async Task CalculateOrderAsync_WithZeroWaypoints_ShouldReturnFailureWithoutHttpCall() {
		// Arrange
		GeoPointValue origin = CreateOrigin();
		List<Waypoint> waypoints = CreateWaypoints(0);

		// Act
		Result<OrderedRoute> result = await _calculator.CalculateOrderAsync(
			origin, waypoints, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().NotBeNull();
		_handler.RequestCount.Should().Be(0);
	}

	[Fact]
	public async Task CalculateOrderAsync_WithValidResponse_ShouldReturnOrderedRoute() {
		// Arrange
		GeoPointValue origin = CreateOrigin();
		List<Waypoint> waypoints = CreateWaypoints(3);

		var response = new GoogleMapsDirectionsResponse {
			Status = "OK",
			Routes = new List<Route> {
				new() { WaypointOrder = new List<int> { 2, 0, 1 } }
			}
		};

		_handler.RespondWith(HttpStatusCode.OK, SerializeResponse(response));

		// Act
		Result<OrderedRoute> result = await _calculator.CalculateOrderAsync(
			origin, waypoints, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Waypoints.Should().HaveCount(3);

		result.Value.Waypoints.ElementAt(0).WaypointId.Should().Be(waypoints[2].Id);
		result.Value.Waypoints.ElementAt(0).Sequence.Should().Be(1);

		result.Value.Waypoints.ElementAt(1).WaypointId.Should().Be(waypoints[0].Id);
		result.Value.Waypoints.ElementAt(1).Sequence.Should().Be(2);

		result.Value.Waypoints.ElementAt(2).WaypointId.Should().Be(waypoints[1].Id);
		result.Value.Waypoints.ElementAt(2).Sequence.Should().Be(3);
	}

	[Fact]
	public async Task CalculateOrderAsync_ShouldBuildRequestUrlWithCorrectParameters() {
		// Arrange
		var origin = GeoPointValue.Create(-16.50, -68.15);
		List<Waypoint> waypoints = CreateWaypoints(2);

		var response = new GoogleMapsDirectionsResponse {
			Status = "OK",
			Routes = new List<Route> {
				new() { WaypointOrder = new List<int> { 0, 1 } }
			}
		};

		_handler.RespondWith(HttpStatusCode.OK, SerializeResponse(response));

		// Act
		await _calculator.CalculateOrderAsync(origin, waypoints, CancellationToken.None);

		// Assert
		_handler.LastRequest.Should().NotBeNull();
		string url = _handler.LastRequest!.RequestUri!.ToString();
		url.Should().StartWith("https://maps.googleapis.com/maps/api/directions/json");
		url.Should().Contain("origin=-16.5,-68.15");
		url.Should().Contain("destination=-16.5,-68.15");
		url.Should().Contain("waypoints=optimize:true");
		url.Should().Contain("key=test-api-key");
	}

	[Fact]
	public async Task CalculateOrderAsync_WithStatusNotOk_ShouldReturnFailure() {
		// Arrange
		GeoPointValue origin = CreateOrigin();
		List<Waypoint> waypoints = CreateWaypoints(2);

		var response = new GoogleMapsDirectionsResponse {
			Status = "ZERO_RESULTS",
			Routes = new List<Route>()
		};

		_handler.RespondWith(HttpStatusCode.OK, SerializeResponse(response));

		// Act
		Result<OrderedRoute> result = await _calculator.CalculateOrderAsync(
			origin, waypoints, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().NotBeNull();
	}

	[Fact]
	public async Task CalculateOrderAsync_WithOkStatusButNoRoutes_ShouldReturnFailure() {
		// Arrange
		GeoPointValue origin = CreateOrigin();
		List<Waypoint> waypoints = CreateWaypoints(2);

		var response = new GoogleMapsDirectionsResponse {
			Status = "OK",
			Routes = new List<Route>()
		};

		_handler.RespondWith(HttpStatusCode.OK, SerializeResponse(response));

		// Act
		Result<OrderedRoute> result = await _calculator.CalculateOrderAsync(
			origin, waypoints, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().NotBeNull();
		result.Error.Code.Should().Be("GoogleMapsError");
	}

	[Fact]
	public async Task CalculateOrderAsync_WithMismatchedWaypointOrderCount_ShouldReturnFailure() {
		// Arrange
		GeoPointValue origin = CreateOrigin();
		List<Waypoint> waypoints = CreateWaypoints(3);

		var response = new GoogleMapsDirectionsResponse {
			Status = "OK",
			Routes = new List<Route> {
				new() { WaypointOrder = new List<int> { 0, 1 } }
			}
		};

		_handler.RespondWith(HttpStatusCode.OK, SerializeResponse(response));

		// Act
		Result<OrderedRoute> result = await _calculator.CalculateOrderAsync(
			origin, waypoints, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().NotBeNull();
	}

	[Fact]
	public async Task CalculateOrderAsync_WithHttpException_ShouldReturnFailure() {
		// Arrange
		GeoPointValue origin = CreateOrigin();
		List<Waypoint> waypoints = CreateWaypoints(2);

		_handler.ThrowOnRequest(new HttpRequestException("Network error"));

		// Act
		Result<OrderedRoute> result = await _calculator.CalculateOrderAsync(
			origin, waypoints, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().NotBeNull();
	}

	[Fact]
	public async Task CalculateOrderAsync_WithInvalidJsonResponse_ShouldReturnFailure() {
		// Arrange
		GeoPointValue origin = CreateOrigin();
		List<Waypoint> waypoints = CreateWaypoints(2);

		_handler.RespondWith(HttpStatusCode.OK, "not a valid json {{{{");

		// Act
		Result<OrderedRoute> result = await _calculator.CalculateOrderAsync(
			origin, waypoints, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().NotBeNull();
	}

	[Fact]
	public async Task CalculateOrderAsync_WithHttpErrorStatusCode_ShouldReturnFailure() {
		// Arrange
		GeoPointValue origin = CreateOrigin();
		List<Waypoint> waypoints = CreateWaypoints(2);

		_handler.RespondWith(HttpStatusCode.InternalServerError, "");

		// Act
		Result<OrderedRoute> result = await _calculator.CalculateOrderAsync(
			origin, waypoints, CancellationToken.None);

		// Assert
		result.IsFailure.Should().BeTrue();
		result.Error.Should().NotBeNull();
	}

	[Theory]
	[InlineData(-16.50, -68.15, "-16.5", "-68.15")]
	[InlineData(0.0, 0.0, "0", "0")]
	[InlineData(45.123456, -120.987654, "45.123456", "-120.987654")]
	public async Task CalculateOrderAsync_ShouldFormatCoordinatesWithDotDecimalSeparator(
		double lat, double lng, string expectedLat, string expectedLng
	) {
		// Arrange
		var origin = GeoPointValue.Create(lat, lng);
		List<Waypoint> waypoints = CreateWaypoints(2);

		var response = new GoogleMapsDirectionsResponse {
			Status = "OK",
			Routes = new List<Route> {
				new() { WaypointOrder = new List<int> { 0, 1 } }
			}
		};
		_handler.RespondWith(HttpStatusCode.OK, SerializeResponse(response));

		// Act
		await _calculator.CalculateOrderAsync(origin, waypoints, CancellationToken.None);

		// Assert
		string url = _handler.LastRequest!.RequestUri!.ToString();
		url.Should().Contain($"origin={expectedLat},{expectedLng}");
	}

	[Fact]
	public async Task CalculateOrderAsync_WithNonInvariantCulture_ShouldStillUseDotDecimalSeparator() {
		// Arrange
		CultureInfo originalCulture = CultureInfo.CurrentCulture;
		CultureInfo.CurrentCulture = new CultureInfo("es-BO");

		try {
			var origin = GeoPointValue.Create(-16.50, -68.15);
			List<Waypoint> waypoints = CreateWaypoints(2);

			var response = new GoogleMapsDirectionsResponse {
				Status = "OK",
				Routes = new List<Route> {
					new() { WaypointOrder = new List<int> { 0, 1 } }
				}
			};
			_handler.RespondWith(HttpStatusCode.OK, SerializeResponse(response));

			// Act
			await _calculator.CalculateOrderAsync(origin, waypoints, CancellationToken.None);

			// Assert
			string url = _handler.LastRequest!.RequestUri!.ToString();
			url.Should().Contain("origin=-16.5,-68.15");
			url.Should().NotContain("-16,5");
		} finally {
			CultureInfo.CurrentCulture = originalCulture;
		}
	}
}
