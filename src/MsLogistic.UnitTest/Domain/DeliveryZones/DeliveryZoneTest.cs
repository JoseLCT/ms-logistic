using FluentAssertions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.DeliveryZones.Entities;
using MsLogistic.Domain.DeliveryZones.Errors;
using MsLogistic.Domain.Shared.ValueObjects;
using Xunit;

namespace MsLogistic.UnitTest.Domain.DeliveryZones;

public class DeliveryZoneTest
{
    private static BoundariesValue CreateValidBoundaries()
    {
        var points = new List<GeoPointValue>
        {
            GeoPointValue.Create(-17.7833, -63.1821),
            GeoPointValue.Create(-17.7833, -63.1621),
            GeoPointValue.Create(-17.7633, -63.1621),
            GeoPointValue.Create(-17.7633, -63.1821)
        };
        return BoundariesValue.Create(points);
    }

    #region Create

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var code = "ABC-123";
        var name = "North Zone";
        var boundaries = CreateValidBoundaries();

        // Act
        var zone = DeliveryZone.Create(driverId, code, name, boundaries);

        // Assert
        zone.Should().NotBeNull();
        zone.Id.Should().NotBe(Guid.Empty);
        zone.DriverId.Should().Be(driverId);
        zone.Code.Should().Be(code);
        zone.Name.Should().Be(name);
        zone.Boundaries.Should().Be(boundaries);
    }

    [Fact]
    public void Create_WithoutDriver_ShouldSucceed()
    {
        // Arrange
        var code = "XYZ-999";
        var name = "South Zone";
        var boundaries = CreateValidBoundaries();

        // Act
        var zone = DeliveryZone.Create(null, code, name, boundaries);

        // Assert
        zone.Should().NotBeNull();
        zone.DriverId.Should().BeNull();
        zone.Code.Should().Be(code);
        zone.Name.Should().Be(name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCode_ShouldThrowDomainException(string invalidCode)
    {
        // Arrange
        var boundaries = CreateValidBoundaries();

        // Act
        Action act = () => DeliveryZone.Create(null, invalidCode, "Zone", boundaries);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(DeliveryZoneErrors.CodeIsRequired);
    }

    [Theory]
    [InlineData("ABC123")]
    [InlineData("AB-123")]
    [InlineData("ABCD-123")]
    [InlineData("ABC-12")]
    [InlineData("ABC-1234")]
    [InlineData("abc-123")]
    [InlineData("123-ABC")]
    public void Create_WithInvalidCodeFormat_ShouldThrowDomainException(string invalidCode)
    {
        // Arrange
        var boundaries = CreateValidBoundaries();

        // Act
        Action act = () => DeliveryZone.Create(null, invalidCode, "Zone", boundaries);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(DeliveryZoneErrors.CodeFormatIsInvalid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowDomainException(string invalidName)
    {
        // Arrange
        var boundaries = CreateValidBoundaries();

        // Act
        Action act = () => DeliveryZone.Create(null, "ABC-123", invalidName, boundaries);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(DeliveryZoneErrors.NameIsRequired);
    }

    #endregion

    #region SetDriverId

    [Fact]
    public void SetDriverId_WithValidId_ShouldUpdateDriver()
    {
        // Arrange
        var zone = DeliveryZone.Create(null, "ABC-123", "Zone", CreateValidBoundaries());
        var driverId = Guid.NewGuid();

        // Act
        zone.SetDriverId(driverId);

        // Assert
        zone.DriverId.Should().Be(driverId);
    }

    [Fact]
    public void SetDriverId_WithNull_ShouldRemoveDriver()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        var zone = DeliveryZone.Create(driverId, "ABC-123", "Zone", CreateValidBoundaries());

        // Act
        zone.SetDriverId(null);

        // Assert
        zone.DriverId.Should().BeNull();
    }

    #endregion

    #region SetCode

    [Fact]
    public void SetCode_WithValidCode_ShouldUpdateCode()
    {
        // Arrange
        var zone = DeliveryZone.Create(null, "ABC-123", "Zone", CreateValidBoundaries());
        var newCode = "XYZ-999";

        // Act
        zone.SetCode(newCode);

        // Assert
        zone.Code.Should().Be(newCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetCode_WithInvalidCode_ShouldThrowDomainException(string invalidCode)
    {
        // Arrange
        var zone = DeliveryZone.Create(null, "ABC-123", "Zone", CreateValidBoundaries());

        // Act
        Action act = () => zone.SetCode(invalidCode);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(DeliveryZoneErrors.CodeIsRequired);
    }

    [Theory]
    [InlineData("ABC123")]
    [InlineData("abc-123")]
    [InlineData("AB-123")]
    public void SetCode_WithInvalidFormat_ShouldThrowDomainException(string invalidCode)
    {
        // Arrange
        var zone = DeliveryZone.Create(null, "ABC-123", "Zone", CreateValidBoundaries());

        // Act
        Action act = () => zone.SetCode(invalidCode);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(DeliveryZoneErrors.CodeFormatIsInvalid);
    }

    #endregion

    #region SetName

    [Fact]
    public void SetName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var zone = DeliveryZone.Create(null, "ABC-123", "North", CreateValidBoundaries());
        var newName = "South Zone";

        // Act
        zone.SetName(newName);

        // Assert
        zone.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetName_WithInvalidName_ShouldThrowDomainException(string invalidName)
    {
        // Arrange
        var zone = DeliveryZone.Create(null, "ABC-123", "Zone", CreateValidBoundaries());

        // Act
        Action act = () => zone.SetName(invalidName);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.Error.Should().Be(DeliveryZoneErrors.NameIsRequired);
    }

    #endregion

    #region SetBoundaries

    [Fact]
    public void SetBoundaries_WithValidBoundaries_ShouldUpdateBoundaries()
    {
        // Arrange
        var zone = DeliveryZone.Create(null, "ABC-123", "Zone", CreateValidBoundaries());
        var newPoints = new List<GeoPointValue>
        {
            GeoPointValue.Create(-17.8000, -63.2000),
            GeoPointValue.Create(-17.8000, -63.1800),
            GeoPointValue.Create(-17.7800, -63.1800),
            GeoPointValue.Create(-17.7800, -63.2000)
        };
        var newBoundaries = BoundariesValue.Create(newPoints);

        // Act
        zone.SetBoundaries(newBoundaries);

        // Assert
        zone.Boundaries.Should().Be(newBoundaries);
        zone.Boundaries.Coordinates.Should().HaveCountGreaterThanOrEqualTo(4);
    }

    #endregion
}