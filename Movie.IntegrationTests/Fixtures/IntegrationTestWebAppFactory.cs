using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using movie_shop_asp.Server.Infrastructure;

namespace Movie.IntegrationTests.Fixtures;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlFixture _dbFixture = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // 기존 DbContext 등록 제거
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<MovieShopContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // 테스트용 DbContext 등록
            services.AddDbContext<MovieShopContext>(options =>
            {
                options.UseNpgsql(_dbFixture.ConnectionString, b =>
                {
                    b.MigrationsAssembly(typeof(MovieShopContext).Assembly.GetName().Name);
                    b.MigrationsHistoryTable("__EFMigrationsHistory", "public");
                });
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbFixture.InitializeAsync();

        // 마이그레이션 적용
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieShopContext>();
        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _dbFixture.DisposeAsync();
    }
}