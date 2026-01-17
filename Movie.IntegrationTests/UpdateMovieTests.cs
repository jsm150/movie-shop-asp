using Microsoft.Extensions.DependencyInjection;
using Movie.API.Application.Commands;
using Movie.Infrastructure;
using Movie.IntegrationTests.Fixtures;
using movie_shop_asp.Server.Infrastructure;
using movie_shop_asp.Server.Movie.API.Application.Commands;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Movie.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class UpdateMovieTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task UpdateMovie_WhenMovieExists_ReturnsOk_And_UpdatesDatabase()
    {
        // arrange: 먼저 등록
        var register = new RegisterMovieCommand
        {
            Title = "업데이트 대상 영화",
            Director = "감독A",
            Genres = ["Action"],
            RuntimeMinutes = 120,
            AdienceRating = Movie.Domain.Aggregate.AdienceRating.ALL,
            Synopsis = "old synopsis",
            ReleaseDate = DateTimeOffset.UtcNow.AddDays(10),
            Casts =
            [
                new ActorDto
                {
                    Name = "배우A",
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
            var db = scope.ServiceProvider.GetRequiredService<MovieShopContext>();
            var entity = db.Movies.Single(m => m.MovieInfo.Title == register.Title);

            movieId = entity.MovieId;

            Assert.Equal(register.Director, entity.MovieInfo.Director);
            Assert.Equal(register.RuntimeMinutes, entity.MovieInfo.RuntimeMinutes);
            Assert.Equal(register.Synopsis, entity.MovieInfo.Synopsis);
        }

        var update = new UpdateMovieCommand
        {
            MovieId = movieId,
            Title = "업데이트 완료 영화",
            Director = "감독B",
            Genres = ["Drama", "Thriller"],
            RuntimeMinutes = 140,
            AdienceRating = Movie.Domain.Aggregate.AdienceRating.ALL,
            Synopsis = "new synopsis",
            ReleaseDate = DateTimeOffset.UtcNow.AddDays(20),
            Casts =
            [
                new ActorDto
                {
                    Name = "배우B",
                    DateOfBirth = DateTimeOffset.UtcNow.AddYears(-28),
                    National = "US",
                    Role = "주연"
                },
                new ActorDto
                {
                    Name = "배우C",
                    DateOfBirth = DateTimeOffset.UtcNow.AddYears(-35),
                    National = "KR",
                    Role = "조연"
                }
            ]
        };

        // act
        var updateResponse = await Client.PutAsJsonAsync("/api/movie", update);

        // assert (HTTP)
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        // assert (DB): 값이 실제로 바뀌었는지 검증
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieShopContext>();
            var updated = db.Movies.Single(m => m.MovieId == movieId);

            Assert.Equal(update.Title, updated.MovieInfo.Title);
            Assert.Equal(update.Director, updated.MovieInfo.Director);
            Assert.Equal(update.RuntimeMinutes, updated.MovieInfo.RuntimeMinutes);
            Assert.Equal(update.Synopsis, updated.MovieInfo.Synopsis);
            Assert.Equal(update.AdienceRating, updated.MovieInfo.AdienceRating);

            Assert.Equal(update.Genres.Count, updated.MovieInfo.Genres.Count);
            Assert.True(update.Genres.All(g => updated.MovieInfo.Genres.Contains(g)));

            Assert.True(
                (updated.MovieInfo.ReleaseDate - update.ReleaseDate.UtcDateTime).Duration() < TimeSpan.FromMilliseconds(1));

            Assert.Equal(update.Casts.Count, updated.MovieInfo.Casts.Count);
            Assert.True(update.Casts.All(c => updated.MovieInfo.Casts.Any(a =>
                a.Name == c.Name &&
                a.Role == c.Role &&
                a.National == c.National &&
                a.DateOfBirth - c.DateOfBirth.UtcDateTime < TimeSpan.FromMilliseconds(1))));
        }
    }

    [Fact]
    public async Task UpdateMovie_WhenMovieDoesNotExist_ReturnsBadRequest_And_DoesNotChangeDatabase()
    {
        // arrange: 기준 데이터 1건 등록(업데이트 실패 시에도 유지되어야 함)
        var register = new RegisterMovieCommand
        {
            Title = "업데이트 실패 영향 없음 영화",
            Director = "감독",
            Genres = ["Drama"],
            RuntimeMinutes = 100,
            AdienceRating = Movie.Domain.Aggregate.AdienceRating.ALL,
            Synopsis = "synopsis",
            ReleaseDate = DateTimeOffset.UtcNow.AddDays(1),
            Casts =
            [
                new ActorDto
                {
                    Name = "배우",
                    DateOfBirth = DateTimeOffset.UtcNow.AddYears(-25),
                    National = "KR",
                    Role = "주연"
                }
            ]
        };

        var registerResponse = await Client.PostAsJsonAsync("/api/movie", register);
        registerResponse.EnsureSuccessStatusCode();

        long existingMovieId;
        int beforeCount;
        string beforeTitle;
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieShopContext>();
            var movie = db.Movies.Single(m => m.MovieInfo.Title == register.Title);

            existingMovieId = movie.MovieId;
            beforeTitle = movie.MovieInfo.Title;
            beforeCount = db.Movies.Count();
        }

        // act: 존재하지 않는 ID로 업데이트 시도
        var update = new UpdateMovieCommand
        {
            MovieId = 99999999,
            Title = "변경 시도",
            Director = "감독X",
            Genres = ["Action"],
            RuntimeMinutes = 90,
            AdienceRating = Movie.Domain.Aggregate.AdienceRating.ALL,
            Synopsis = "synopsis",
            ReleaseDate = DateTimeOffset.UtcNow.AddDays(2),
            Casts =
            [
                new ActorDto
                {
                    Name = "배우X",
                    DateOfBirth = DateTimeOffset.UtcNow.AddYears(-20),
                    National = "KR",
                    Role = "주연"
                }
            ]
        };

        var response = await Client.PutAsJsonAsync("/api/movie", update);

        // assert (HTTP): MovieDomainException -> 400
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // assert (DB): 기존 데이터가 영향 없음을 검증
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieShopContext>();

            var afterCount = db.Movies.Count();
            Assert.Equal(beforeCount, afterCount);

            var stillExists = db.Movies.Any(m => m.MovieId == existingMovieId && m.MovieInfo.Title == beforeTitle);
            Assert.True(stillExists);
        }
    }
}