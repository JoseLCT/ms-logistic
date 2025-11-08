using MediatR;
using MsLogistic.Core.Abstractions;
using MsLogistic.Core.Results;
using MsLogistic.Domain.Customer.Repositories;
using MsLogistic.Domain.Order.Repositories;
using MsLogistic.Domain.Shared.ValueObjects;

namespace MsLogistic.Application.Order.CreateOrder;

internal class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork
    )
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, readOnly: true);
        if (customer is null)
        {
            return Result.Failure<Guid>(
                Error.NotFound(
                    code: "customer_not_found",
                    structuredMessage: $"Customer with id {request.CustomerId} was not found."
                )
            );
        }

        try
        {
            var geoPoint = new GeoPointValue(
                latitude: request.DeliveryLocation.Latitude,
                longitude: request.DeliveryLocation.Longitude
            );

            var order = new Domain.Order.Entities.Order(
                customerId: request.CustomerId,
                scheduledDeliveryDate: request.ScheduledDeliveryDate,
                deliveryAddress: request.DeliveryAddress,
                deliveryLocation: geoPoint
            );

            foreach (var item in request.Items)
            {
                order.AddItem(
                    productId: item.ProductId,
                    quantity: item.Quantity
                );
            }

            await _orderRepository.AddAsync(order);

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Success(order.Id);
        }
        catch (DomainException dex)
        {
            return Result.Failure<Guid>(
                Error.Problem(
                    code: dex.Error.Code,
                    structuredMessage: dex.Error.StructuredMessage
                )
            );
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(
                Error.Problem(
                    code: "create_order_failed",
                    structuredMessage: ex.Message
                )
            );
        }
    }
}