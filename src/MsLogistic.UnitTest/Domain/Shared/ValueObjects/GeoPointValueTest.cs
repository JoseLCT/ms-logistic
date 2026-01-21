using FluentAssertions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Domain.Shared.ValueObjects;

public class GeoPointValueTest
{
    #region Create

    [Fact]
    public void Create_WithValidCoordinates_ShouldSucceed()
    {
        // Arrange
        var latitude = -17.7833;
        var longitude = -63.1821;

        // Act
        var geoPoint = GeoPointValue.Create(latitude, longitude);

        // Assert
        geoPoint.Should().NotBeNull();
        geoPoint.Latitude.Should().Be(latitude);
        geoPoint.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(90, 180)]
    [InlineData(-90, -180)]
    [InlineData(45.5, -122.6)]
    [InlineData(-33.8688, 151.2093)]
    [InlineData(51.5074, -0.1278)]
    public void Create_WithValidBoundaryCoordinates_ShouldSucceed(double latitude, double longitude)
    {
        // Act
        var geoPoint = GeoPointValue.Create(latitude, longitude);

        // Assert
        geoPoint.Should().NotBeNull();
        geoPoint.Latitude.Should().Be(latitude);
        geoPoint.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    [InlineData(-91)]
    [InlineData(91)]
    [InlineData(-100)]
    [InlineData(100)]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    public void Create_WithInvalidLatitude_ShouldThrowDomainException(double invalidLatitude)
    {
        // Arrange
        var validLongitude = 0.0;

        // Act
        Action act = () => GeoPointValue.Create(invalidLatitude, validLongitude);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(GeoPointErrors.LatitudeOutOfRange);
    }

    [Theory]
    [InlineData(-180.1)]
    [InlineData(180.1)]
    [InlineData(-181)]
    [InlineData(181)]
    [InlineData(-200)]
    [InlineData(200)]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    public void Create_WithInvalidLongitude_ShouldThrowDomainException(double invalidLongitude)
    {
        // Arrange
        var validLatitude = 0.0;

        // Act
        Action act = () => GeoPointValue.Create(validLatitude, invalidLongitude);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(GeoPointErrors.LongitudeOutOfRange);
    }

    #endregion
}