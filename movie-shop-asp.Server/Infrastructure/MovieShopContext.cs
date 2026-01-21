using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Movie.Infrastructure;
using Movie.Infrastructure.Configurations;
using Screening.Infrastructure.Configurations;
using Screening.Infrastructure;
using System.Data;

using MovieEntity = Movie.Domain.Aggregate.Movie;
using Screening.Domain.Aggregate.ScreenAggregate;


namespace movie_shop_asp.Server.Infrastructure
{
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
            modelBuilder.ApplyConfiguration(new Movie.Infrastructure.Configurations.MovieEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new Screening.Infrastructure.Configurations.MovieEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new TheaterEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ScreenEntityTypeConfiguration());
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            _ = await base.SaveChangesAsync(cancellationToken);
            return true;
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
}
