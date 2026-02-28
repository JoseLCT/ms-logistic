using FluentValidation;
using MsLogistic.Domain.Customers.Repositories;
using MsLogistic.Domain.Orders.Errors;
using MsLogistic.Domain.Products.Repositories;
using MsLogistic.Domain.Shared.Errors;

namespace MsLogistic.Application.Orders.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand> {
    private const int MaxDeliveryAddressLength = 500;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;

    public CreateOrderCommandValidator(
        ICustomerRepository customerRepository,
        IProductRepository productRepository) {
        _customerRepository = customerRepository;
        _productRepository = productRepository;

        var customerNotFoundError = CommonErrors.NotFound("Customer");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .MustAsync(CustomerExists)
            .WithMessage(customerNotFoundError.Message)
            .WithErrorCode(customerNotFoundError.Code);

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage(OrderErrors.ItemsAreRequired.Message)
            .WithErrorCode(OrderErrors.ItemsAreRequired.Code)
            .DependentRules(() => {
                RuleFor(x => x.Items)
                    .MustAsync(AllProductsExist)
                    .WithMessage(OrderErrors.ProductsNotFound.Message)
                    .WithErrorCode(OrderErrors.ProductsNotFound.Code);
            });

        RuleForEach(x => x.Items)
            .ChildRules(item => {
                item.RuleFor(i => i.ProductId)
                    .NotEmpty();

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0)
                    .WithMessage(OrderItemErrors.QuantityMustBeGreaterThanZero.Message)
                    .WithErrorCode(OrderItemErrors.QuantityMustBeGreaterThanZero.Code);
            });

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty()
            .WithMessage(OrderErrors.DeliveryAddressIsRequired.Message)
            .WithErrorCode(OrderErrors.DeliveryAddressIsRequired.Code)
            .MaximumLength(MaxDeliveryAddressLength)
            .WithMessage(OrderErrors.DeliveryAddressTooLong(MaxDeliveryAddressLength).Message)
            .WithErrorCode(OrderErrors.DeliveryAddressTooLong(MaxDeliveryAddressLength).Code);
    }

    private async Task<bool> CustomerExists(Guid customerId, CancellationToken ct) {
        var customer = await _customerRepository.GetByIdAsync(customerId, ct);
        return customer != null;
    }

    private async Task<bool> AllProductsExist(
        IReadOnlyCollection<CreateOrderItemDto> items,
        CancellationToken ct
    ) {
        var productIds = items.Select(i => i.ProductId).ToHashSet();
        var existingProducts = await _productRepository.GetByIdsAsync(productIds, ct);
        return existingProducts.Count == productIds.Count;
    }
}
