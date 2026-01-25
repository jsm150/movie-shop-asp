using BuildingBlocks.API.Application;
using MediatR;
using Microsoft.EntityFrameworkCore;
using movie_shop_asp.Server.Infrastructure;

namespace movie_shop_asp.Server.Application.Behaviors;

public class TransactionBehavior<TCommand, TResponse>(MovieShopContext _dbContext) : IPipelineBehavior<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    public async Task<TResponse> Handle(TCommand request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
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
