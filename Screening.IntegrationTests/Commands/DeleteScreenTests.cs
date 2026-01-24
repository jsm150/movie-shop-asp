using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.IntegrationTest.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using movie_shop_asp.Server.Infrastructure;
using Screening.API.Application.Commands;
using Screening.Domain.Aggregate.MovieAggregate;
using Screening.Domain.Aggregate.ScreenAggregate;
using Screening.Domain.Aggregate.TheaterAggregate;
using Xunit;

using MovieEntity = Screening.Domain.Aggregate.MovieAggregate.Movie;
using TheaterEntity = Screening.Domain.Aggregate.TheaterAggregate.Theater;

namespace Screening.IntegrationTests.Commands;

[Collection(nameof(IntegrationTestCollection))]
public class DeleteScreenTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task DeleteScreen_WithValidScheduledScreen_ShouldDeleteSuccessfully()
    {
        // Arrange
        var (screenId, _, _) = await SeedScreenAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/screen/{screenId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<bool>();
        result.Should().BeTrue();

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieShopContext>();
        var deletedScreen = await context.Screens.FirstOrDefaultAsync(s => s.ScreenId == screenId);

        deletedScreen.Should().BeNull();
    }

    [Fact]
    public async Task DeleteScreen_WhenScreenNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var nonExistentScreenId = 999999L;

        // Act
        var response = await Client.DeleteAsync($"/api/screen/{nonExistentScreenId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteScreen_WhenScreenIsNotScheduled_ShouldReturnBadRequest()
    {
        // Arrange
        var (screenId, _, _) = await SeedScreenAsync();

        // 상영 상태를 SCHEDULED가 아닌 상태로 변경
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MovieShopContext>();
            var screen = await context.Screens.FirstAsync(s => s.ScreenId == screenId);

            // 리플렉션을 사용하여 Status 속성 변경
            var statusProperty = typeof(Screen).GetProperty(nameof(Screen.Status));
            statusProperty!.SetValue(screen, ScreenStatus.ON_SALE);
            await context.SaveChangesAsync();
        }

        // Act
        var response = await Client.DeleteAsync($"/api/screen/{screenId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<(long ScreenId, long TheaterId, long MovieId)> SeedScreenAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieShopContext>();

        var movie = new MovieEntity { MovieId = 1, MovieStatus = MovieStatus.NOW_SHOWING };
        context.ScreeningMovies.Add(movie);
        await context.SaveChangesAsync();

        var theater = new TheaterEntity(1, [new SeatCode("A1"), new SeatCode("A2")]);
        context.Theaters.Add(theater);
        await context.SaveChangesAsync();

        var createCommand = new CreateScreenCommand
        {
            MovieId = movie.MovieId,
            TheaterId = theater.TheaterId,
            StartTime = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero),
            SalesStartAt = new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            SalesEndAt = new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero)
        };

        var response = await Client.PostAsJsonAsync("/api/screen", createCommand);
        response.EnsureSuccessStatusCode();
        var screenId = await response.Content.ReadFromJsonAsync<long>();

        return (screenId, theater.TheaterId, movie.MovieId);
    }
}