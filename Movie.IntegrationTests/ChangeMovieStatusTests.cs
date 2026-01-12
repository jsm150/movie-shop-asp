using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Movie.Domain.Aggregate;
using Movie.Infrastructure;
using Movie.IntegrationTests.Fixtures;
using movie_shop_asp.Server.Movie.API.Application.Commands;
using Xunit;

namespace Movie.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class ChangeMovieStatusTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task ChangeMovieStatus_Preparing_To_CommingSoon_ReturnsOk_And_UpdatesDatabase()
    {
        // arrange
        var register = new RegisterMovieCommand
        {
            Title = "상태변경 테스트 영화 1",
            Director = "감독",
            Genres = ["Action"],
            RuntimeMinutes = 120,
            AdienceRating = AdienceRating.ALL,
            Synopsis = "synopsis",
            ReleaseDate = DateTimeOffset.UtcNow.AddDays(10),
            Casts =
            [
                new ActorDto
                {
                    Name = "배우",
                    DateOfBirth = DateTimeOffset.UtcNow.AddYears(-30),
                    National = "KR",
                    Role = "주연"
                }
            ]
        };

        var registerResponse = await Client.PostAsJsonAsync("/api/movie", register);
        registerResponse.EnsureSuccessStatusCode();

        long movieId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieContext>();
            var entity = db.Movies.Single(m => m.MovieInfo.Title == register.Title);

            movieId = entity.MovieId;
            Assert.Equal(MovieStatus.PREPARING, entity.MovieStatus);
        }

        // act
        var command = new ChangeMovieStatusCommand
        {
            MovieId = movieId,
            Status = MovieStatus.COMMING_SOON
        };

        var response = await Client.PutAsJsonAsync("/api/movie/status", command);

        // assert (HTTP)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // assert (DB)
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieContext>();
            var updated = db.Movies.Single(m => m.MovieId == movieId);

            Assert.Equal(MovieStatus.COMMING_SOON, updated.MovieStatus);
        }
    }

    [Fact]
    public async Task ChangeMovieStatus_CommingSoon_To_NowShowing_ReturnsOk_And_UpdatesDatabase()
    {
        // arrange: PREPARING -> COMMING_SOON 까지 선행
        var register = new RegisterMovieCommand
        {
            Title = "상태변경 테스트 영화 2",
            Director = "감독",
            Genres = ["Action"],
            RuntimeMinutes = 120,
            AdienceRating = AdienceRating.ALL,
            Synopsis = "synopsis",
            ReleaseDate = DateTimeOffset.UtcNow.AddDays(10),
            Casts =
            [
                new ActorDto
                {
                    Name = "배우",
                    DateOfBirth = DateTimeOffset.UtcNow.AddYears(-30),
                    National = "KR",
                    Role = "주연"
                }
            ]
        };

        var registerResponse = await Client.PostAsJsonAsync("/api/movie", register);
        registerResponse.EnsureSuccessStatusCode();

        long movieId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieContext>();
            movieId = db.Movies.Single(m => m.MovieInfo.Title == register.Title).MovieId;
        }

        var toCommingSoon = new ChangeMovieStatusCommand { MovieId = movieId, Status = MovieStatus.COMMING_SOON };
        var toCommingSoonResponse = await Client.PutAsJsonAsync("/api/movie/status", toCommingSoon);
        toCommingSoonResponse.EnsureSuccessStatusCode();

        // act
        var toNowShowing = new ChangeMovieStatusCommand { MovieId = movieId, Status = MovieStatus.NOW_SHOWING };
        var response = await Client.PutAsJsonAsync("/api/movie/status", toNowShowing);

        // assert (HTTP)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // assert (DB)
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieContext>();
            var updated = db.Movies.Single(m => m.MovieId == movieId);

            Assert.Equal(MovieStatus.NOW_SHOWING, updated.MovieStatus);
        }
    }

    [Fact]
    public async Task ChangeMovieStatus_NowShowing_To_Ended_ReturnsOk_And_UpdatesDatabase()
    {
        // arrange: PREPARING -> COMMING_SOON -> NOW_SHOWING 까지 선행
        var register = new RegisterMovieCommand
        {
            Title = "상태변경 테스트 영화 3",
            Director = "감독",
            Genres = ["Action"],
            RuntimeMinutes = 120,
            AdienceRating = AdienceRating.ALL,
            Synopsis = "synopsis",
            ReleaseDate = DateTimeOffset.UtcNow.AddDays(10),
            Casts =
            [
                new ActorDto
                {
                    Name = "배우",
                    DateOfBirth = DateTimeOffset.UtcNow.AddYears(-30),
                    National = "KR",
                    Role = "주연"
                }
            ]
        };

        var registerResponse = await Client.PostAsJsonAsync("/api/movie", register);
        registerResponse.EnsureSuccessStatusCode();

        long movieId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieContext>();
            movieId = db.Movies.Single(m => m.MovieInfo.Title == register.Title).MovieId;
        }

        var toCommingSoon = new ChangeMovieStatusCommand { MovieId = movieId, Status = MovieStatus.COMMING_SOON };
        var toCommingSoonResponse = await Client.PutAsJsonAsync("/api/movie/status", toCommingSoon);
        toCommingSoonResponse.EnsureSuccessStatusCode();

        var toNowShowing = new ChangeMovieStatusCommand { MovieId = movieId, Status = MovieStatus.NOW_SHOWING };
        var toNowShowingResponse = await Client.PutAsJsonAsync("/api/movie/status", toNowShowing);
        toNowShowingResponse.EnsureSuccessStatusCode();

        // act
        var toEnded = new ChangeMovieStatusCommand { MovieId = movieId, Status = MovieStatus.ENDED };
        var response = await Client.PutAsJsonAsync("/api/movie/status", toEnded);

        // assert (HTTP)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // assert (DB)
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieContext>();
            var updated = db.Movies.Single(m => m.MovieId == movieId);

            Assert.Equal(MovieStatus.ENDED, updated.MovieStatus);
        }
    }

    [Fact]
    public async Task ChangeMovieStatus_WhenMovieDoesNotExist_ReturnsBadRequest()
    {
        // act
        var command = new ChangeMovieStatusCommand
        {
            MovieId = 99999999,
            Status = MovieStatus.COMMING_SOON
        };

        var response = await Client.PutAsJsonAsync("/api/movie/status", command);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangeMovieStatus_WhenTransitionIsInvalid_ReturnsBadRequest_And_DoesNotChangeDatabase()
    {
        // arrange: PREPARING 상태에서 곧바로 NOW_SHOWING으로 바꾸려 시도(불법 전이)
        var register = new RegisterMovieCommand
        {
            Title = "상태변경 실패 테스트 영화",
            Director = "감독",
            Genres = ["Action"],
            RuntimeMinutes = 120,
            AdienceRating = AdienceRating.ALL,
            Synopsis = "synopsis",
            ReleaseDate = DateTimeOffset.UtcNow.AddDays(10),
            Casts =
            [
                new ActorDto
                {
                    Name = "배우",
                    DateOfBirth = DateTimeOffset.UtcNow.AddYears(-30),
                    National = "KR",
                    Role = "주연"
                }
            ]
        };

        var registerResponse = await Client.PostAsJsonAsync("/api/movie", register);
        registerResponse.EnsureSuccessStatusCode();

        long movieId;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieContext>();
            var entity = db.Movies.Single(m => m.MovieInfo.Title == register.Title);
            movieId = entity.MovieId;

            Assert.Equal(MovieStatus.PREPARING, entity.MovieStatus);
        }

        // act
        var command = new ChangeMovieStatusCommand
        {
            MovieId = movieId,
            Status = MovieStatus.NOW_SHOWING
        };

        var response = await Client.PutAsJsonAsync("/api/movie/status", command);

        // assert (HTTP)
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // assert (DB): 여전히 PREPARING
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieContext>();
            var updated = db.Movies.Single(m => m.MovieId == movieId);

            Assert.Equal(MovieStatus.PREPARING, updated.MovieStatus);
        }
    }
}