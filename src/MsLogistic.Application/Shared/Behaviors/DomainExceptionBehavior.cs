using System.Linq.Expressions;
using MediatR;
using MsLogistic.Core.Results;

namespace MsLogistic.Application.Shared.Behaviors;

public class DomainExceptionBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result {
    private static readonly Func<Error, TResponse> FailureFactory = CreateFailureFactory();

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct
    ) {
        try {
            return await next();
        } catch (DomainException ex) {
            return FailureFactory(ex.Error);
        }
    }

    private static Func<Error, TResponse> CreateFailureFactory() {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result)) {
            return error => (TResponse)Result.Failure(error);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>)) {
            var valueType = responseType.GetGenericArguments()[0];

            var failureMethod = typeof(Result)
                .GetMethod(nameof(Result.Failure), 1, new[] { typeof(Error) })!
                .MakeGenericMethod(valueType);

            var errorParam = Expression.Parameter(typeof(Error), "error");
            var callExpression = Expression.Call(failureMethod, errorParam);
            var convertExpression = Expression.Convert(callExpression, typeof(TResponse));

            return Expression.Lambda<Func<Error, TResponse>>(
                convertExpression,
                errorParam
            ).Compile();
        }

        throw new InvalidOperationException(
            $"Unsupported response type: {responseType.Name}. " +
            $"Expected Result or Result<T>.");
    }
}
