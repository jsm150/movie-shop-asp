using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.IntegrationTest.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using movie_shop_asp.Server.Infrastructure;
using Screening.API.Application.Commands;
using Screening.Domain.Aggregate.MovieAggregate;
using Screening.Domain.Aggregate.TheaterAggregate;
using Xunit;

using MovieEntity = Screening.Domain.Aggregate.MovieAggregate.Movie;
using TheaterEntity = Screening.Domain.Aggregate.TheaterAggregate.Theater;

namespace Screening.IntegrationTests.Commands;

[Collection(nameof(IntegrationTestCollection))]
public class UpdateScreenTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task UpdateScreen_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var (screenId, theaterId, _) = await SeedScreenAsync();

        var command = new UpdateScreenCommand
        {
            ScreenId = screenId,
            StartTime = new DateTimeOffset(2026, 2, 1, 14, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2026, 2, 1, 16, 0, 0, TimeSpan.Zero),
            SalesStartAt = new DateTimeOffset(2026, 2, 1, 12, 0, 0, TimeSpan.Zero),
            SalesEndAt = new DateTimeOffset(2026, 2, 1, 13, 30, 0, TimeSpan.Zero)
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/screen/{screenId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieShopContext>();
        var updatedScreen = await context.Screens.FirstOrDefaultAsync(s => s.ScreenId == screenId);

        updatedScreen.Should().NotBeNull();
        updatedScreen!.StartTime.Should().Be(command.StartTime);
        updatedScreen.EndTime.Should().Be(command.EndTime);
        updatedScreen.SalesStartAt.Should().Be(command.SalesStartAt);
        updatedScreen.SalesEndAt.Should().Be(command.SalesEndAt);
    }

    [Fact]
    public async Task UpdateScreen_WhenTimeConflictsWithAnotherScreen_ShouldReturnBadRequest()
    {
        // Arrange
        var (existingScreenId, theaterId, movieId) = await SeedScreenAsync();
        var secondScreenId = await SeedScreenAsync(
            theaterId,
            movieId,
            startTime: new DateTimeOffset(2026, 1, 1, 14, 0, 0, TimeSpan.Zero),
            endTime: new DateTimeOffset(2026, 1, 1, 16, 0, 0, TimeSpan.Zero));

        // 두 번째 상영을 첫 번째 상영 시간과 충돌하도록 수정 시도
        var command = new UpdateScreenCommand
        {
            ScreenId = secondScreenId,
            StartTime = new DateTimeOffset(2026, 1, 1, 10, 30, 0, TimeSpan.Zero), // 기존 상영(10:00~12:00)과 충돌
            EndTime = new DateTimeOffset(2026, 1, 1, 12, 30, 0, TimeSpan.Zero),
            SalesStartAt = new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            SalesEndAt = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero)
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/screen/{secondScreenId}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateScreen_WhenScreenNotFound_ShouldReturnBadRequest()
    {
        // Arrange
        var nonExistentScreenId = 999999L;
        var command = new UpdateScreenCommand
        {
            ScreenId = nonExistentScreenId,
            StartTime = new DateTimeOffset(2026, 2, 1, 10, 0, 0, TimeSpan.Zero),
            EndTime = new DateTimeOffset(2026, 2, 1, 12, 0, 0, TimeSpan.Zero),
            SalesStartAt = new DateTimeOffset(2026, 2, 1, 8, 0, 0, TimeSpan.Zero),
            SalesEndAt = new DateTimeOffset(2026, 2, 1, 9, 30, 0, TimeSpan.Zero)
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/screen/{nonExistentScreenId}", command);

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

    private async Task<long> SeedScreenAsync(
        long theaterId,
        long moiveId,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieShopContext>();

        var createCommand = new CreateScreenCommand
        {
            MovieId = moiveId,
            TheaterId = theaterId,
            StartTime = startTime,
            EndTime = endTime,
            SalesStartAt = startTime.AddHours(-2),
            SalesEndAt = startTime.AddMinutes(-30)
        };

        var response = await Client.PostAsJsonAsync("/api/screen", createCommand);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<long>();
    }
}