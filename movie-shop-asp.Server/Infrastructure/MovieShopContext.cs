using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

using MovieEntity = Movie.Domain.Aggregate.Movie;
using Screening.Domain.Aggregate.ScreenAggregate;
using Movie.API.Infrastructure.Extensions;
using Movie.API.Infrastructure;
using Screening.Infrastructure;
using Screening.Infrastructure.Extensions;

namespace movie_shop_asp.Server.Infrastructure;

public class MovieShopContext(DbContextOptions<MovieShopContext> options) :
    DbContext(options),
    IMovieContext,
    IScreeningContext
{
    public DbSet<MovieEntity> Movies { get; set; }
    public DbSet<Screening.Domain.Aggregate.MovieAggregate.Movie> ScreeningMovies { get; set; }

    public DbSet<Screening.Domain.Aggregate.TheaterAggregate.Theater> Theaters { get; set; }

    public DbSet<Screen> Screens { get; set; }

    private IDbContextTransaction? _currentTransaction;
    public bool HasActiveTransaction => _currentTransaction != null;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyMovieModuleConfigurations();
        modelBuilder.ApplyScreeningModuleConfigurations();
    }

    public async Task SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await base.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction?> BeginTransactionAsync()
    {
        if (HasActiveTransaction) return null;

        _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        return _currentTransaction;
    }

    public async Task CommitTransactionAsync(IDbContextTransaction? transaction)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        if (transaction != _currentTransaction) throw new InvalidOperationException($"Transaction {transaction.TransactionId} is not current");

        try
        {
            await SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            RollbackTransaction();
            throw;
        }
        finally
        {
            if (HasActiveTransaction)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void RollbackTransaction()
    {
        try
        {
            _currentTransaction?.Rollback();
        }
        finally
        {
            if (HasActiveTransaction)
            {
                _currentTransaction!.Dispose();
                _currentTransaction = null;
            }
        }
    }
}
