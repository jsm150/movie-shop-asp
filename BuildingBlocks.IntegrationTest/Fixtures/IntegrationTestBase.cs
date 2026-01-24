using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using movie_shop_asp.Server.Infrastructure;
using Respawn;
using System.Net.Http.Json;
using Xunit;

namespace BuildingBlocks.IntegrationTest.Fixtures;

public abstract class IntegrationTestBase(IntegrationTestWebAppFactory factory) : IAsyncLifetime
{
    protected readonly IntegrationTestWebAppFactory Factory = factory;
    protected readonly HttpClient Client = factory.CreateClient();
    private Respawner? _respawner;

    public async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieShopContext>();
        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["Movie", "Screening"]
        });

        await _respawner.ResetAsync(connection);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var response = await Client.PostAsJsonAsync(url, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }
}