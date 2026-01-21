using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MsLogistic.Application.Products.CreateProduct;
using MsLogistic.Application.Products.GetAllProducts;
using MsLogistic.Application.Products.GetProductById;
using MsLogistic.Application.Products.UpdateProduct;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.IntegrationTest.Fixtures;
using MsLogistic.WebApi.Contracts.V1.Products;
using Xunit;

namespace MsLogistic.IntegrationTest.Controllers.V1;

public class ProductControllerTest : IClassFixture<WebApplicationFactoryFixture>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactoryFixture _factory;
    private readonly List<Guid> _createdProductIds = [];

    public ProductControllerTest(WebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (!_createdProductIds.Any())
        {
            return;
        }

        using var scope = _factory.Services.CreateScope();
        var persistenceDb = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

        var productsToDelete = await persistenceDb.Products
            .Where(p => _createdProductIds.Contains(p.Id))
            .ToListAsync();

        if (productsToDelete.Any())
        {
            persistenceDb.Products.RemoveRange(productsToDelete);
            await persistenceDb.SaveChangesAsync();
        }
    }

    #region Create Product Tests

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var contract = new CreateProductContract
        {
            Name = "Maruchan",
            Description = "Sopa instantánea sabor pollo"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", contract);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var productId = await response.Content.ReadFromJsonAsync<Guid>();
        productId.Should().NotBeEmpty();
        _createdProductIds.Add(productId);

        await VerifyProductInPersistenceDb(productId, contract.Name, contract.Description);
    }

    [Fact]
    public async Task CreateProduct_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var contract = new CreateProductContract
        {
            Name = "",
            Description = "Producto sin nombre"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", contract);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Detail.Should().Contain("Name");
    }

    [Fact]
    public async Task CreateProduct_WithNullName_ShouldReturnBadRequest()
    {
        // Arrange
        var contract = new CreateProductContract
        {
            Name = null!,
            Description = "Producto sin nombre"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", contract);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Update Product Tests

    [Fact]
    public async Task UpdateProduct_WithValidData_ShouldUpdateProductSuccessfully()
    {
        // Arrange
        var createContract = new CreateProductContract
        {
            Name = "Pollo",
            Description = "Pollo con papas"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", createContract);
        var productId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        _createdProductIds.Add(productId);

        var updateContract = new UpdateProductContract
        {
            Name = "Pollo Asado",
            Description = "Pollo asado con papas"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/products/{productId}", updateContract);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await VerifyProductInPersistenceDb(productId, "Pollo Asado", "Pollo asado con papas");
    }

    [Fact]
    public async Task UpdateProduct_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var updateContract = new UpdateProductContract
        {
            Name = "Producto Inexistente",
            Description = "Este producto no existe"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/products/{nonExistingId}", updateContract);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Delete Product Tests

    [Fact]
    public async Task DeleteProduct_WithExistingId_ShouldDeleteProductSuccessfully()
    {
        // Arrange
        var createContract = new CreateProductContract
        {
            Name = "Producto a eliminar",
            Description = "Este producto será eliminado"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", createContract);
        var productId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        _createdProductIds.Add(productId);

        // Act
        var response = await _client.DeleteAsync($"/api/v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        await VerifyProductDoesNotExist(productId);
    }

    [Fact]
    public async Task DeleteProduct_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/products/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Get Product Tests

    [Fact]
    public async Task GetProductById_WithExistingId_ShouldReturnProduct()
    {
        // Arrange
        var createContract = new CreateProductContract
        {
            Name = "Pizza Margarita",
            Description = "Pizza con tomate y queso"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", createContract);
        var productId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        _createdProductIds.Add(productId);

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var product = await response.Content.ReadFromJsonAsync<ProductDetailDto>();
        product.Should().NotBeNull();
        product.Id.Should().Be(productId);
        product.Name.Should().Be("Pizza Margarita");
        product.Description.Should().Be("Pizza con tomate y queso");
    }

    [Fact]
    public async Task GetProductById_WithNonExistingId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/products/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnListOfProducts()
    {
        // Arrange
        var product1 = new CreateProductContract
        {
            Name = "Producto 1",
            Description = "Descripción 1"
        };
        var product2 = new CreateProductContract
        {
            Name = "Producto 2",
            Description = "Descripción 2"
        };
        var product3 = new CreateProductContract
        {
            Name = "Producto 3",
            Description = "Descripción 3"
        };

        var response1 = await _client.PostAsJsonAsync("/api/v1/products", product1);
        var response2 = await _client.PostAsJsonAsync("/api/v1/products", product2);
        var response3 = await _client.PostAsJsonAsync("/api/v1/products", product3);

        _createdProductIds.Add(await response1.Content.ReadFromJsonAsync<Guid>());
        _createdProductIds.Add(await response2.Content.ReadFromJsonAsync<Guid>());
        _createdProductIds.Add(await response3.Content.ReadFromJsonAsync<Guid>());

        // Act
        var response = await _client.GetAsync("/api/v1/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var products = await response.Content.ReadFromJsonAsync<List<ProductSummaryDto>>();
        products.Should().NotBeNull();
        products.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    #endregion

    #region Complete CRUD Flow

    [Fact]
    public async Task CompleteProductCRUDFlow_ShouldWorkEndToEnd()
    {
        // CREATE
        var createContract = new CreateProductContract
        {
            Name = "Test Product CRUD",
            Description = "Descripción de prueba"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", createContract);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var productId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        _createdProductIds.Add(productId);

        // READ
        var getResponse = await _client.GetAsync($"/api/v1/products/{productId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await getResponse.Content.ReadFromJsonAsync<ProductDetailDto>();
        product.Should().NotBeNull();
        product.Name.Should().Be("Test Product CRUD");

        // UPDATE
        var updateContract = new UpdateProductContract
        {
            Name = "Test Product Updated",
            Description = "Descripción actualizada"
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/v1/products/{productId}", updateContract);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // VERIFY UPDATE
        var getUpdatedResponse = await _client.GetAsync($"/api/v1/products/{productId}");
        var updatedProduct = await getUpdatedResponse.Content.ReadFromJsonAsync<ProductDetailDto>();
        updatedProduct.Should().NotBeNull();
        updatedProduct.Name.Should().Be("Test Product Updated");
        updatedProduct.Description.Should().Be("Descripción actualizada");

        // DELETE
        var deleteResponse = await _client.DeleteAsync($"/api/v1/products/{productId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // VERIFY DELETION
        var getDeletedResponse = await _client.GetAsync($"/api/v1/products/{productId}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Helper Methods

    private async Task VerifyProductInPersistenceDb(Guid productId, string expectedName, string? expectedDescription)
    {
        using var scope = _factory.Services.CreateScope();
        var persistenceDb = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

        var product = await persistenceDb.Products
            .FirstOrDefaultAsync(p => p.Id == productId);

        product.Should().NotBeNull();
        product!.Name.Should().Be(expectedName);
        product.Description.Should().Be(expectedDescription);
    }

    private async Task VerifyProductDoesNotExist(Guid productId)
    {
        using var scope = _factory.Services.CreateScope();
        var persistenceDb = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

        var productExists = await persistenceDb.Products
            .AnyAsync(p => p.Id == productId);

        productExists.Should().BeFalse();
    }

    #endregion
}