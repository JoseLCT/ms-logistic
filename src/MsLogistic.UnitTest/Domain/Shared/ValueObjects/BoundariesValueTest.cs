using FluentAssertions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Shared.Errors;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Domain.Shared.ValueObjects;

public class BoundariesValueTest
{
    private static List<GeoPointValue> CreateValidTriangle()
    {
        return new List<GeoPointValue>
        {
            GeoPointValue.Create(0, 0),
            GeoPointValue.Create(0, 1),
            GeoPointValue.Create(1, 0)
        };
    }

    private static List<GeoPointValue> CreateValidSquare()
    {
        return new List<GeoPointValue>
        {
            GeoPointValue.Create(0, 0),
            GeoPointValue.Create(0, 1),
            GeoPointValue.Create(1, 1),
            GeoPointValue.Create(1, 0)
        };
    }

    #region Create

    [Fact]
    public void Create_WithValidTriangle_ShouldSucceed()
    {
        // Arrange
        var coordinates = CreateValidTriangle();

        // Act
        var boundaries = BoundariesValue.Create(coordinates);

        // Assert
        boundaries.Should().NotBeNull();
        boundaries.Coordinates.Should().HaveCount(4);
        boundaries.Coordinates[0].Should().Be(boundaries.Coordinates[^1]);
    }

    [Fact]
    public void Create_WithValidSquare_ShouldSucceed()
    {
        // Arrange
        var coordinates = CreateValidSquare();

        // Act
        var boundaries = BoundariesValue.Create(coordinates);

        // Assert
        boundaries.Should().NotBeNull();
        boundaries.Coordinates.Should().HaveCount(5);
    }

    [Fact]
    public void Create_WithAlreadyClosedPolygon_ShouldNotAddExtraPoint()
    {
        // Arrange
        var coordinates = new List<GeoPointValue>
        {
            GeoPointValue.Create(0, 0),
            GeoPointValue.Create(0, 1),
            GeoPointValue.Create(1, 0),
            GeoPointValue.Create(0, 0)
        };

        // Act
        var boundaries = BoundariesValue.Create(coordinates);

        // Assert
        boundaries.Coordinates.Should().HaveCount(4);
        boundaries.Coordinates[0].Should().Be(boundaries.Coordinates[^1]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Create_WithInsufficientPoints_ShouldThrowDomainException(int pointCount)
    {
        var coordinates = new List<GeoPointValue>();
        for (int i = 0; i < pointCount; i++)
        {
            coordinates.Add(GeoPointValue.Create(i, i));
        }

        // Act
        Action act = () => BoundariesValue.Create(coordinates);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(BoundariesErrors.InsufficientPoints(3));
    }

    [Fact]
    public void Create_WithConsecutiveDuplicatePoints_ShouldThrowDomainException()
    {
        // Arrange
        var coordinates = new List<GeoPointValue>
        {
            GeoPointValue.Create(0, 0),
            GeoPointValue.Create(0, 0),
            GeoPointValue.Create(1, 1)
        };

        // Act
        Action act = () => BoundariesValue.Create(coordinates);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(BoundariesErrors.ConsecutiveDuplicatePoints);
    }

    #endregion

    #region Contains

    [Fact]
    public void Contains_WithPointInside_ShouldReturnTrue()
    {
        // Arrange
        var boundaries = BoundariesValue.Create(CreateValidSquare());
        var pointInside = GeoPointValue.Create(0.5, 0.5);

        // Act
        var result = boundaries.Contains(pointInside);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithPointOutside_ShouldReturnFalse()
    {
        // Arrange
        var boundaries = BoundariesValue.Create(CreateValidSquare());
        var pointOutside = GeoPointValue.Create(2, 2);

        // Act
        var result = boundaries.Contains(pointOutside);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetCenter

    [Fact]
    public void GetCenter_WithSquare_ShouldReturnCenterPoint()
    {
        // Arrange
        var boundaries = BoundariesValue.Create(CreateValidSquare());

        // Act
        var center = boundaries.GetCenter();

        // Assert
        center.Latitude.Should().BeApproximately(0.5, 0.0001);
        center.Longitude.Should().BeApproximately(0.5, 0.0001);
    }

    [Fact]
    public void GetCenter_WithTriangle_ShouldReturnCentroid()
    {
        // Arrange
        var boundaries = BoundariesValue.Create(CreateValidTriangle());

        // Act
        var center = boundaries.GetCenter();

        // Assert
        center.Latitude.Should().BeApproximately(0.333333, 0.0001);
        center.Longitude.Should().BeApproximately(0.333333, 0.0001);
    }

    [Fact]
    public void GetCenter_ShouldExcludeClosingPoint()
    {
        var coordinates = new List<GeoPointValue>
        {
            GeoPointValue.Create(0, 0),
            GeoPointValue.Create(0, 2),
            GeoPointValue.Create(1, 2),
            GeoPointValue.Create(1, 0)
        };
        var boundaries = BoundariesValue.Create(coordinates);

        // Act
        var center = boundaries.GetCenter();

        center.Latitude.Should().BeApproximately(0.5, 0.0001);
        center.Longitude.Should().BeApproximately(1.0, 0.0001);
    }

    #endregion
}