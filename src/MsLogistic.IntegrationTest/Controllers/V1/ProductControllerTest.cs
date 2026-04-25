using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MsLogistic.Application.Products.GetAllProducts;
using MsLogistic.Application.Products.GetProductById;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;
using MsLogistic.IntegrationTest.Fixtures;
using MsLogistic.IntegrationTest.Helpers;
using MsLogistic.WebApi.Contracts.V1.Products;
using Xunit;

namespace MsLogistic.IntegrationTest.Controllers.V1;

public class ProductControllerTest : IClassFixture<WebApplicationFactoryFixture>, IAsyncLifetime {
	private const string BaseUrl = "/api/logistic/v1/products";
	private readonly HttpClient _client;
	private readonly WebApplicationFactoryFixture _factory;
	private readonly List<Guid> _createdProductIds = [];

	public ProductControllerTest(WebApplicationFactoryFixture factory) {
		_factory = factory;
		_client = factory.CreateClient();
	}

	public Task InitializeAsync() => Task.CompletedTask;

	public async Task DisposeAsync() {
		if (_createdProductIds.Count == 0) {
			return;
		}

		using IServiceScope scope = _factory.Services.CreateScope();
		PersistenceDbContext persistenceDb = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

		List<ProductPersistenceModel> productsToDelete = await persistenceDb.Products
			.Where(p => _createdProductIds.Contains(p.Id))
			.ToListAsync();

		if (productsToDelete.Count != 0) {
			persistenceDb.Products.RemoveRange(productsToDelete);
			await persistenceDb.SaveChangesAsync();
		}
	}

	#region Create Product Tests

	[Fact]
	public async Task CreateProduct_WithValidData_ShouldCreateProductSuccessfully() {
		// Arrange
		var contract = new CreateProductContract {
			Name = "Maruchan",
			Description = "Sopa instantanea sabor pollo"
		};

		// Act
		HttpResponseMessage response = await _client.PostAsJsonAsync(BaseUrl, contract);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Created);

		ApiResponse<Guid>? result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
		result.Should().NotBeNull();
		result!.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeEmpty();
		_createdProductIds.Add(result.Value);

		await VerifyProductInPersistenceDb(result.Value, contract.Name, contract.Description);
	}

	[Fact]
	public async Task CreateProduct_WithEmptyName_ShouldReturnBadRequest() {
		// Arrange
		var contract = new CreateProductContract {
			Name = "",
			Description = "Producto sin nombre"
		};

		// Act
		HttpResponseMessage response = await _client.PostAsJsonAsync(BaseUrl, contract);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task CreateProduct_WithNullName_ShouldReturnBadRequest() {
		// Arrange
		var contract = new CreateProductContract {
			Name = null!,
			Description = "Producto sin nombre"
		};

		// Act
		HttpResponseMessage response = await _client.PostAsJsonAsync(BaseUrl, contract);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	#endregion

	#region Update Product Tests

	[Fact]
	public async Task UpdateProduct_WithValidData_ShouldUpdateProductSuccessfully() {
		// Arrange
		Guid productId = await CreateProductAndTrack("Pollo", "Pollo con papas");

		var updateContract = new UpdateProductContract {
			Name = "Pollo Asado",
			Description = "Pollo asado con papas"
		};

		// Act
		HttpResponseMessage response = await _client.PutAsJsonAsync($"{BaseUrl}/{productId}", updateContract);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		await VerifyProductInPersistenceDb(productId, "Pollo Asado", "Pollo asado con papas");
	}

	[Fact]
	public async Task UpdateProduct_WithNonExistingId_ShouldReturnNotFound() {
		// Arrange
		var nonExistingId = Guid.NewGuid();
		var updateContract = new UpdateProductContract {
			Name = "Producto Inexistente",
			Description = "Este producto no existe"
		};

		// Act
		HttpResponseMessage response = await _client.PutAsJsonAsync($"{BaseUrl}/{nonExistingId}", updateContract);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	#endregion

	#region Delete Product Tests

	[Fact]
	public async Task DeleteProduct_WithExistingId_ShouldDeleteProductSuccessfully() {
		// Arrange
		Guid productId = await CreateProductAndTrack("Producto a eliminar", "Este producto sera eliminado");

		// Act
		HttpResponseMessage response = await _client.DeleteAsync($"{BaseUrl}/{productId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		await VerifyProductDoesNotExist(productId);
	}

	[Fact]
	public async Task DeleteProduct_WithNonExistingId_ShouldReturnNotFound() {
		// Arrange
		var nonExistingId = Guid.NewGuid();

		// Act
		HttpResponseMessage response = await _client.DeleteAsync($"{BaseUrl}/{nonExistingId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	#endregion

	#region Get Product Tests

	[Fact]
	public async Task GetProductById_WithExistingId_ShouldReturnProduct() {
		// Arrange
		Guid productId = await CreateProductAndTrack("Pizza Margarita", "Pizza con tomate y queso");

		// Act
		HttpResponseMessage response = await _client.GetAsync($"{BaseUrl}/{productId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		ApiResponse<ProductDetailDto>? result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDetailDto>>();
		result.Should().NotBeNull();
		result!.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value!.Id.Should().Be(productId);
		result.Value.Name.Should().Be("Pizza Margarita");
		result.Value.Description.Should().Be("Pizza con tomate y queso");
	}

	[Fact]
	public async Task GetProductById_WithNonExistingId_ShouldReturnNotFound() {
		// Arrange
		var nonExistingId = Guid.NewGuid();

		// Act
		HttpResponseMessage response = await _client.GetAsync($"{BaseUrl}/{nonExistingId}");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task GetAllProducts_ShouldReturnListOfProducts() {
		// Arrange
		await CreateProductAndTrack("Producto 1", "Descripcion 1");
		await CreateProductAndTrack("Producto 2", "Descripcion 2");
		await CreateProductAndTrack("Producto 3", "Descripcion 3");

		// Act
		HttpResponseMessage response = await _client.GetAsync(BaseUrl);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);

		ApiResponse<List<ProductSummaryDto>>? result =
			await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductSummaryDto>>>();
		result.Should().NotBeNull();
		result!.IsSuccess.Should().BeTrue();
		result.Value.Should().NotBeNull();
		result.Value.Should().HaveCountGreaterThanOrEqualTo(3);
	}

	#endregion

	#region Complete CRUD Flow

	[Fact]
	public async Task CompleteProductCRUDFlow_ShouldWorkEndToEnd() {
		// CREATE
		Guid productId = await CreateProductAndTrack("Test Product CRUD", "Descripcion de prueba");

		// READ
		HttpResponseMessage getResponse = await _client.GetAsync($"{BaseUrl}/{productId}");
		getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
		ApiResponse<ProductDetailDto>? product =
			await getResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDetailDto>>();
		product.Should().NotBeNull();
		product!.Value!.Name.Should().Be("Test Product CRUD");

		// UPDATE
		var updateContract = new UpdateProductContract {
			Name = "Test Product Updated",
			Description = "Descripcion actualizada"
		};
		HttpResponseMessage updateResponse = await _client.PutAsJsonAsync($"{BaseUrl}/{productId}", updateContract);
		updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		// VERIFY UPDATE
		HttpResponseMessage getUpdatedResponse = await _client.GetAsync($"{BaseUrl}/{productId}");
		ApiResponse<ProductDetailDto>? updatedProduct =
			await getUpdatedResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDetailDto>>();
		updatedProduct.Should().NotBeNull();
		updatedProduct!.Value!.Name.Should().Be("Test Product Updated");
		updatedProduct.Value.Description.Should().Be("Descripcion actualizada");

		// DELETE
		HttpResponseMessage deleteResponse = await _client.DeleteAsync($"{BaseUrl}/{productId}");
		deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

		// VERIFY DELETION
		HttpResponseMessage getDeletedResponse = await _client.GetAsync($"{BaseUrl}/{productId}");
		getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	#endregion

	#region Helper Methods

	private async Task<Guid> CreateProductAndTrack(string name, string? description = null) {
		var contract = new CreateProductContract { Name = name, Description = description };
		HttpResponseMessage response = await _client.PostAsJsonAsync(BaseUrl, contract);
		response.StatusCode.Should().Be(HttpStatusCode.Created);

		ApiResponse<Guid>? result = await response.Content.ReadFromJsonAsync<ApiResponse<Guid>>();
		result.Should().NotBeNull();
		result!.Value.Should().NotBeEmpty();
		_createdProductIds.Add(result.Value);
		return result.Value;
	}

	private async Task VerifyProductInPersistenceDb(Guid productId, string expectedName,
		string? expectedDescription) {
		using IServiceScope scope = _factory.Services.CreateScope();
		PersistenceDbContext persistenceDb = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

		ProductPersistenceModel? product = await persistenceDb.Products
			.FirstOrDefaultAsync(p => p.Id == productId);

		product.Should().NotBeNull();
		product!.Name.Should().Be(expectedName);
		product.Description.Should().Be(expectedDescription);
	}

	private async Task VerifyProductDoesNotExist(Guid productId) {
		using IServiceScope scope = _factory.Services.CreateScope();
		PersistenceDbContext persistenceDb = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

		bool productExists = await persistenceDb.Products
			.AnyAsync(p => p.Id == productId);

		productExists.Should().BeFalse();
	}

	#endregion
}
