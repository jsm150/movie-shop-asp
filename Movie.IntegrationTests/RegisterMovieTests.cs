using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Movie.Domain.Aggregate;
using Movie.Infrastructure;
using Movie.IntegrationTests.Fixtures;
using movie_shop_asp.Server.Movie.API.Application.Commands;
using System.Net;
using System.Net.Http.Json;

namespace Movie.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class RegisterMovieTests(IntegrationTestWebAppFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task ValidCommand_ShouldSaveToDatabase()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var response = await Client.PostAsJsonAsync("/api/movie", command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieContext>();

        var savedMovie = await context.Movies
            .FirstOrDefaultAsync(m => m.MovieInfo.Title == command.Title);

        Assert.NotNull(savedMovie);
        Assert.Equal(command.Title, savedMovie.MovieInfo.Title);
        Assert.Equal(command.Director, savedMovie.MovieInfo.Director);
        Assert.Equal(command.RuntimeMinutes, savedMovie.MovieInfo.RuntimeMinutes);
        Assert.Equal(command.Synopsis, savedMovie.MovieInfo.Synopsis);
        Assert.Equal(command.AdienceRating, savedMovie.MovieInfo.AdienceRating);
        Assert.Equal(command.ReleaseDate, savedMovie.MovieInfo.ReleaseDate);
        
        // Genres 검증
        Assert.Equal(command.Genres.Count, savedMovie.MovieInfo.Genres.Count);
        foreach (var genre in command.Genres)
        {
            Assert.Contains(genre, savedMovie.MovieInfo.Genres);
        }
        
        // Casts 검증
        Assert.Equal(command.Casts.Count, savedMovie.MovieInfo.Casts.Count);
        foreach (var castDto in command.Casts)
        {
            var savedActor = savedMovie.MovieInfo.Casts.FirstOrDefault(a => a.Name == castDto.Name);
            Assert.NotNull(savedActor);
            Assert.Equal(castDto.Role, savedActor.Role);
            Assert.Equal(castDto.DateOfBirth, savedActor.DateOfBirth);
            Assert.Equal(castDto.National, savedActor.National);
        }
    }

    [Fact]
    public async Task DuplicateTitle_ShouldReturnError()
    {
        // Arrange
        var command = CreateValidCommand();
        await Client.PostAsJsonAsync("/api/movie", command);

        // Act - 같은 제목으로 다시 등록
        var response = await Client.PostAsJsonAsync("/api/movie", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task WithMultipleActors_ShouldSaveAllActors()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Casts =
            [
                new ActorDto
                {
                    Name = "배우1",
                    Role = "주연",
                    DateOfBirth = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    National = "한국"
                },
                new ActorDto
                {
                    Name = "배우2",
                    Role = "조연",
                    DateOfBirth = new DateTimeOffset(1985, 5, 15, 0, 0, 0, TimeSpan.Zero),
                    National = "미국"
                },
                new ActorDto
                {
                    Name = "배우3",
                    Role = "단역",
                    DateOfBirth = new DateTimeOffset(2000, 12, 25, 0, 0, 0, TimeSpan.Zero),
                    National = "일본"
                }
            ]
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/movie", command);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieContext>();

        var savedMovie = await context.Movies
            .FirstOrDefaultAsync(m => m.MovieInfo.Title == command.Title);

        Assert.NotNull(savedMovie);
        Assert.Equal(3, savedMovie.MovieInfo.Casts.Count);
        Assert.Contains(savedMovie.MovieInfo.Casts, a => a.Name == "배우1" && a.Role == "주연");
        Assert.Contains(savedMovie.MovieInfo.Casts, a => a.Name == "배우2" && a.Role == "조연");
    }

    [Fact]
    public async Task InvalidCommand_ShouldReturnBadRequest()
    {
        // Arrange - 빈 제목
        var command = CreateValidCommand() with { Title = "" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/movie", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static RegisterMovieCommand CreateValidCommand() => new()
    {
        Title = $"테스트 영화 {Guid.NewGuid()}",
        Director = "테스트 감독",
        Genres = ["액션", "SF"],
        RuntimeMinutes = 120,
        AdienceRating = AdienceRating.ALL,
        Synopsis = "테스트 영화 줄거리입니다.",
        ReleaseDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero),
        Casts =
        [
            new ActorDto
            {
                Name = "테스트 배우",
                Role = "주연",
                DateOfBirth = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero),
                National = "한국"
            }
        ]
    };
}