using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MsLogistic.Application.Product.CreateProduct;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.IntegrationTest.Fixtures;
using Xunit;

namespace MsLogistic.IntegrationTest.Controllers;

public class ProductControllerTest : IClassFixture<WebApplicationFactoryFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactoryFixture _factory;

    public ProductControllerTest(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var persistenceDb = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

        await persistenceDb.Database.BeginTransactionAsync();

        persistenceDb.Product.RemoveRange(persistenceDb.Product);
        await persistenceDb.SaveChangesAsync();

        await persistenceDb.Database.CommitTransactionAsync();
    }

    [Fact]
    public async Task CreateProduct_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var command = new CreateProductCommand
        (
            Name: "Test Product",
            Description: "This is a test product"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/Product", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdProduct = await response.Content.ReadFromJsonAsync<Guid>();
        createdProduct.Should().NotBeEmpty();

        await VerifyProductExistsInDatabase(createdProduct);
    }
    
    [Fact]
    public async Task CreateProduct_ShouldReturnBadRequest_WhenNameIsMissing()
    {
        // Arrange
        var command = new CreateProductCommand
        (
            Name: "",
            Description: "This is a test product without a name"
        );
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/Product", command);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task VerifyProductExistsInDatabase(Guid productId)
    {
        using var scope = _factory.Services.CreateScope();
        var persistenceDb = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

        var productExists = await persistenceDb.Product
            .AnyAsync(p => p.Id == productId);

        productExists.Should().BeTrue();
    }
}