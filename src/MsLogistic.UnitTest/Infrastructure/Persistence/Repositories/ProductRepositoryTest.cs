using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Infrastructure.Persistence.DomainModel;
using MsLogistic.Infrastructure.Persistence.Repositories;
using Xunit;

namespace MsLogistic.UnitTest.Infrastructure.Persistence.Repositories;

public class ProductRepositoryTest : IDisposable {
	private readonly DomainDbContext _dbContext;
	private readonly ProductRepository _repository;

	public ProductRepositoryTest() {
		DbContextOptions<DomainDbContext> options = new DbContextOptionsBuilder<DomainDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		_dbContext = new DomainDbContext(options);
		_repository = new ProductRepository(_dbContext);
	}

	public void Dispose() {
		_dbContext.Database.EnsureDeleted();
		_dbContext.Dispose();
	}

	private static Product CreateValidProduct(
		string name = "Chicken Soup",
		string description = "Delicious chicken soup",
		Guid? externalId = null
	) {
		return Product.Create(
			name: name,
			description: description,
			externalId: externalId
		);
	}

	#region GetByIdAsync

	[Fact]
	public async Task GetByIdAsync_WhenProductExists_ShouldReturnProduct() {
		// Arrange
		Product product = CreateValidProduct();
		await _dbContext.Products.AddAsync(product);
		await _dbContext.SaveChangesAsync();

		// Act
		Product? result = await _repository.GetByIdAsync(product.Id);

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().Be(product.Id);
		result.Name.Should().Be(product.Name);
		result.Description.Should().Be(product.Description);
	}

	[Fact]
	public async Task GetByIdAsync_WhenProductDoesNotExist_ShouldReturnNull() {
		// Arrange
		var nonExistingId = Guid.NewGuid();

		// Act
		Product? result = await _repository.GetByIdAsync(nonExistingId);

		// Assert
		result.Should().BeNull();
	}

	#endregion

	#region GetByExternalIdAsync

	[Fact]
	public async Task GetByExternalIdAsync_WhenProductExists_ShouldReturnProduct() {
		// Arrange
		var externalId = Guid.NewGuid();
		Product product = CreateValidProduct(externalId: externalId);
		await _dbContext.Products.AddAsync(product);
		await _dbContext.SaveChangesAsync();

		// Act
		Product? result = await _repository.GetByExternalIdAsync(externalId);

		// Assert
		result.Should().NotBeNull();
		result.Id.Should().Be(product.Id);
		result.ExternalId.Should().Be(externalId);
	}

	[Fact]
	public async Task GetByExternalIdAsync_WhenProductDoesNotExist_ShouldReturnNull() {
		// Arrange
		var nonExistingExternalId = Guid.NewGuid();

		// Act
		Product? result = await _repository.GetByExternalIdAsync(nonExistingExternalId);

		// Assert
		result.Should().BeNull();
	}

	#endregion

	#region GetByExternalIdsAsync

	[Fact]
	public async Task GetByExternalIdsAsync_WhenProductsExist_ShouldReturnMatchingProducts() {
		// Arrange
		var externalId1 = Guid.NewGuid();
		var externalId2 = Guid.NewGuid();
		var externalId3 = Guid.NewGuid();

		Product product1 = CreateValidProduct(externalId: externalId1);
		Product product2 = CreateValidProduct(externalId: externalId2);
		Product product3 = CreateValidProduct(externalId: externalId3);
		Product productWithoutExternalId = CreateValidProduct();

		await _dbContext.Products.AddRangeAsync(product1, product2, product3, productWithoutExternalId);
		await _dbContext.SaveChangesAsync();

		var externalIdsToFind = new List<Guid> { externalId1, externalId3 };

		// Act
		IReadOnlyList<Product> result = await _repository.GetByExternalIdsAsync(externalIdsToFind);

		// Assert
		result.Should().HaveCount(2);
		result.Should().Contain(p => p.Id == product1.Id);
		result.Should().Contain(p => p.Id == product3.Id);
		result.Should().NotContain(p => p.Id == product2.Id);
		result.Should().NotContain(p => p.Id == productWithoutExternalId.Id);
	}

	[Fact]
	public async Task GetByExternalIdsAsync_WhenNoMatchingProducts_ShouldReturnEmptyList() {
		// Arrange
		Product product = CreateValidProduct(externalId: Guid.NewGuid());
		await _dbContext.Products.AddAsync(product);
		await _dbContext.SaveChangesAsync();

		var externalIdsToFind = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

		// Act
		IReadOnlyList<Product> result = await _repository.GetByExternalIdsAsync(externalIdsToFind);

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region GetAllAsync

	[Fact]
	public async Task GetAllAsync_WhenProductsExist_ShouldReturnAllProducts() {
		// Arrange
		Product product1 = CreateValidProduct("Pasta Alfredo");
		Product product2 = CreateValidProduct("Beef Stew");
		Product product3 = CreateValidProduct("Vegetable Salad");

		await _dbContext.Products.AddRangeAsync(product1, product2, product3);
		await _dbContext.SaveChangesAsync();

		// Act
		IReadOnlyList<Product> result = await _repository.GetAllAsync();

		// Assert
		result.Should().HaveCount(3);
		result.Should().Contain(p => p.Id == product1.Id);
		result.Should().Contain(p => p.Id == product2.Id);
		result.Should().Contain(p => p.Id == product3.Id);
	}

	[Fact]
	public async Task GetAllAsync_WhenNoProducts_ShouldReturnEmptyList() {
		// Act
		IReadOnlyList<Product> result = await _repository.GetAllAsync();

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region GetByIdsAsync

	[Fact]
	public async Task GetByIdsAsync_WhenProductsExist_ShouldReturnMatchingProducts() {
		// Arrange
		Product product1 = CreateValidProduct();
		Product product2 = CreateValidProduct();
		Product product3 = CreateValidProduct();

		await _dbContext.Products.AddRangeAsync(product1, product2, product3);
		await _dbContext.SaveChangesAsync();

		var idsToFind = new List<Guid> { product1.Id, product3.Id };

		// Act
		IReadOnlyList<Product> result = await _repository.GetByIdsAsync(idsToFind);

		// Assert
		result.Should().HaveCount(2);
		result.Should().Contain(p => p.Id == product1.Id);
		result.Should().Contain(p => p.Id == product3.Id);
		result.Should().NotContain(p => p.Id == product2.Id);
	}

	[Fact]
	public async Task GetByIdsAsync_WhenNoMatchingProducts_ShouldReturnEmptyList() {
		// Arrange
		Product product = CreateValidProduct();
		await _dbContext.Products.AddAsync(product);
		await _dbContext.SaveChangesAsync();

		var idsToFind = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

		// Act
		IReadOnlyList<Product> result = await _repository.GetByIdsAsync(idsToFind);

		// Assert
		result.Should().BeEmpty();
	}

	#endregion

	#region AddAsync

	[Fact]
	public async Task AddAsync_ShouldAddProductToDatabase() {
		// Arrange
		Product product = CreateValidProduct();

		// Act
		await _repository.AddAsync(product);
		await _dbContext.SaveChangesAsync();

		// Assert
		Product? savedProduct = await _dbContext.Products.FindAsync(product.Id);
		savedProduct.Should().NotBeNull();
		savedProduct.Id.Should().Be(product.Id);
		savedProduct.Name.Should().Be(product.Name);
		savedProduct.Description.Should().Be(product.Description);
	}

	#endregion

	#region Update

	[Fact]
	public async Task Update_ShouldUpdateProductInDatabase() {
		// Arrange
		Product product = CreateValidProduct();
		await _dbContext.Products.AddAsync(product);
		await _dbContext.SaveChangesAsync();

		_dbContext.Entry(product).State = EntityState.Detached;

		// Act
		Product? productToUpdate = await _dbContext.Products.FindAsync(product.Id);
		productToUpdate!.SetName("Updated Product Name");
		productToUpdate.SetDescription("Updated Product Description");
		_repository.Update(productToUpdate);
		await _dbContext.SaveChangesAsync();

		// Assert
		Product? updatedProduct = await _dbContext.Products.FindAsync(product.Id);
		updatedProduct!.Name.Should().Be("Updated Product Name");
		updatedProduct.Description.Should().Be("Updated Product Description");
	}

	#endregion

	#region Remove

	[Fact]
	public async Task Remove_ShouldDeleteProductFromDatabase() {
		// Arrange
		Product product = CreateValidProduct();
		await _dbContext.Products.AddAsync(product);
		await _dbContext.SaveChangesAsync();

		// Act
		_repository.Remove(product);
		await _dbContext.SaveChangesAsync();

		// Assert
		Product? deletedProduct = await _dbContext.Products.FindAsync(product.Id);
		deletedProduct.Should().BeNull();
	}

	#endregion
}
