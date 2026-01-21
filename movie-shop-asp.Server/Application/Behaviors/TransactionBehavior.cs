using MediatR;
using Microsoft.EntityFrameworkCore;
using movie_shop_asp.Server.Infrastructure;

namespace movie_shop_asp.Server.Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse>(MovieShopContext _dbContext) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = default(TResponse);

        if (_dbContext.HasActiveTransaction)
        {
            return await next(cancellationToken);
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.BeginTransactionAsync();
            response = await next();
            await _dbContext.CommitTransactionAsync(transaction);
        });

        return response!;
        
    }
}
