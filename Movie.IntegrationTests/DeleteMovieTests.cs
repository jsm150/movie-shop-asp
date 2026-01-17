using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Movie.Infrastructure;
using Movie.IntegrationTests.Fixtures;
using movie_shop_asp.Server.Infrastructure;
using movie_shop_asp.Server.Movie.API.Application.Commands;
using Xunit;

namespace Movie.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class DeleteMovieTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task DeleteMovie_WhenMovieExists_ReturnsOk_And_RemovesFromDatabase()
    {
        // arrange: 먼저 등록
        var register = new RegisterMovieCommand
        {
            Title = "삭제 테스트 영화",
            Director = "감독",
            Genres = ["Action"],
            RuntimeMinutes = 120,
            AdienceRating = Movie.Domain.Aggregate.AdienceRating.ALL,
            Synopsis = "synopsis",
            ReleaseDate = DateTimeOffset.UtcNow.AddDays(1),
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
            var db = scope.ServiceProvider.GetRequiredService<MovieShopContext>();
            movieId = db.Movies.Single(m => m.MovieInfo.Title == register.Title).MovieId;
        }

        // act
        var deleteCommand = new DeleteMovieCommand(movieId);
        var deleteResponse = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/movie")
        {
            Content = JsonContent.Create(deleteCommand)
        });

        // assert (HTTP)
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // assert (DB): 삭제되었는지 검증
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieShopContext>();

            var exists = db.Movies.Any(m => m.MovieId == movieId);
            Assert.False(exists);
        }
    }

    [Fact]
    public async Task DeleteMovie_WhenMovieDoesNotExist_ReturnsBadRequest_And_DoesNotChangeDatabase()
    {
        // arrange: 기준 데이터(삭제 실패 시에도 유지되어야 함) 1건 등록
        var register = new RegisterMovieCommand
        {
            Title = "삭제 실패 영향 없음 영화",
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
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieShopContext>();
            existingMovieId = db.Movies.Single(m => m.MovieInfo.Title == register.Title).MovieId;
            beforeCount = db.Movies.Count();
        }

        // act: 존재하지 않는 ID 삭제 시도
        var deleteCommand = new DeleteMovieCommand(99999999);
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/movie")
        {
            Content = JsonContent.Create(deleteCommand)
        });

        // assert (HTTP): MovieDomainException -> 400
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // assert (DB): 기존 데이터가 영향 없음을 검증
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieShopContext>();

            var afterCount = db.Movies.Count();
            Assert.Equal(beforeCount, afterCount);

            var stillExists = db.Movies.Any(m => m.MovieId == existingMovieId);
            Assert.True(stillExists);
        }
    }
}