using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MsLogistic.Application.Abstractions.Services;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Logistics.ValueObjects;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Infrastructure.External.GoogleMaps;

public class GoogleMapsRouteCalculator : IRouteCalculator {
    private readonly HttpClient _http;
    private readonly IOptions<GoogleMapsOptions> _options;
    private readonly ILogger<GoogleMapsRouteCalculator> _logger;

    public GoogleMapsRouteCalculator(
        HttpClient http,
        IOptions<GoogleMapsOptions> options,
        ILogger<GoogleMapsRouteCalculator> logger
    ) {
        _http = http;
        _options = options;
        _logger = logger;
    }

    public async Task<Result<OrderedRoute>> CalculateOrderAsync(
        GeoPointValue origin,
        IReadOnlyCollection<Waypoint> waypoints,
        CancellationToken ct
    ) {
        if (waypoints.Count < 2) {
            return Result.Failure<OrderedRoute>(Error.Validation(
                code: "InsufficientWaypoints",
                message: "At least two waypoints are required to calculate a route."
            ));
        }

        var waypointsList = waypoints.ToList();
        var middleWaypoints = waypointsList.SkipLast(1).ToList();

        var waypointsParam = string.Join("|", middleWaypoints
            .Select(w => $"{w.Location.Latitude},{w.Location.Longitude}"));

        var baseRequestUrl =
            $"{_options.Value.BaseUrl}/directions/json" +
            $"?origin={origin.Latitude},{origin.Longitude}" +
            $"&destination={origin.Latitude},{origin.Longitude}" +
            $"&waypoints=optimize:true|{waypointsParam}";

        var requestUrl = $"{baseRequestUrl}&key={_options.Value.ApiKey}";

        try {
            _logger.LogDebug("Calling Google Maps API: {Url}", baseRequestUrl);

            var response = await _http.GetFromJsonAsync<GoogleMapsDirectionsResponse>(
                requestUrl,
                cancellationToken: ct
            );

            if (response == null || response.Status != "OK" || response.Routes.Count == 0) {
                _logger.LogError(
                    "Google Maps API returned an error or no routes found. Status: {Status}",
                    response?.Status
                );
                return Result.Failure<OrderedRoute>(Error.Failure(
                    code: "GoogleMapsError",
                    message: "Failed to retrieve route from Google Maps."
                ));
            }

            var route = response.Routes[0];

            if (route.WaypointOrder.Count != waypointsList.Count) {
                return Result.Failure<OrderedRoute>(
                    Error.Failure(
                        code: "InvalidWaypointOrder",
                        message: "Google Maps returned an invalid waypoint order."
                    )
                );
            }

            var orderedWaypoints = route.WaypointOrder
                .Select((index, seq) => new OrderedWaypoint(
                    waypointsList[index].Id,
                    seq + 1
                ))
                .ToList();

            var orderedRoute = new OrderedRoute(orderedWaypoints.AsReadOnly());
            return Result.Success(orderedRoute);
        } catch (Exception ex) {
            _logger.LogError(ex, "Exception occurred while calling Google Maps API.");
            return Result.Failure<OrderedRoute>(Error.Failure(
                code: "GoogleMapsException",
                message: "An exception occurred while retrieving route from Google Maps."
            ));
        }
    }
}
