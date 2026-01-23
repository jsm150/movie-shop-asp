using BuildingBlocks.IntegrationTest.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Movie.API.Application.Commands;
using Movie.Domain.Aggregate;
using movie_shop_asp.Server.Infrastructure;
using System.Net;
using System.Net.Http.Json;

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

    [Fact]
    public async Task DeleteMovie_WhenMovieIsNowShowing_ReturnsBadRequest_And_DoesNotRemoveFromDatabase()
    {
        // arrange: 영화 등록 -> 상태를 COMMING_SOON -> NOW_SHOWING 으로 변경
        var register = new RegisterMovieCommand
        {
            Title = "상영중 삭제 불가 영화",
            Director = "감독",
            Genres = ["Thriller"],
            RuntimeMinutes = 110,
            AdienceRating = Movie.Domain.Aggregate.AdienceRating.ALL,
            Synopsis = "synopsis",
            ReleaseDate = DateTimeOffset.UtcNow.AddDays(1),
            Casts =
            [
                new ActorDto
                {
                    Name = "배우",
                    DateOfBirth = DateTimeOffset.UtcNow.AddYears(-28),
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

        // PREPARING -> COMMING_SOON
        var toCommingSoon = new ChangeMovieStatusCommand()
        {
            MovieId = movieId,
            Status = Movie.Domain.Aggregate.MovieStatus.COMMING_SOON
        };
        var commingSoonResponse = await Client.PutAsJsonAsync("/api/movie/status", toCommingSoon);
        commingSoonResponse.EnsureSuccessStatusCode();

        // COMMING_SOON -> NOW_SHOWING
        var toNowShowing = new ChangeMovieStatusCommand()
        {
            MovieId = movieId,
            Status = Movie.Domain.Aggregate.MovieStatus.NOW_SHOWING
        };
        var nowShowingResponse = await Client.PutAsJsonAsync("/api/movie/status", toNowShowing);
        nowShowingResponse.EnsureSuccessStatusCode();

        // act: 상영중 삭제 시도
        var deleteCommand = new DeleteMovieCommand(movieId);
        var deleteResponse = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/movie")
        {
            Content = JsonContent.Create(deleteCommand)
        });

        // assert (HTTP): MovieDomainException -> 400
        Assert.Equal(HttpStatusCode.BadRequest, deleteResponse.StatusCode);

        // assert (DB): 삭제되지 않아야 함
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieShopContext>();

            var movie = db.Movies.SingleOrDefault(m => m.MovieId == movieId);
            Assert.NotNull(movie);
        }
    }

    [Fact]
    public async Task DeleteMovie_PublishesIntegrationEvent_And_RemovesMovieFromScreeningModule()
    {
        // arrange: 영화 등록 (등록 시 MovieCreatedIntegrationEvent로 ScreeningMovies에도 insert 되어야 함)
        var register = new RegisterMovieCommand
        {
            Title = "통합이벤트 삭제 테스트",
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
            var db = scope.ServiceProvider.GetRequiredService<MovieShopContext>();
            movieId = db.Movies.Single(m => m.MovieInfo.Title == register.Title).MovieId;

            // 등록 통합 이벤트 처리로 ScreeningMovies에도 존재해야 함
            Assert.True(db.ScreeningMovies.Any(m => m.MovieId == movieId));
        }

        // act: 삭제 (DeleteMovieCommandHandler가 MovieDeletedIntegrationEvent를 Add)
        var deleteResponse = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/movie")
        {
            Content = JsonContent.Create(new DeleteMovieCommand(movieId))
        });
        deleteResponse.EnsureSuccessStatusCode();

        // assert: Movie 모듈에서 삭제 + Screening 모듈에서도 삭제
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MovieShopContext>();

            Assert.False(db.Movies.Any(m => m.MovieId == movieId));
            Assert.False(db.ScreeningMovies.Any(m => m.MovieId == movieId));
        }
    }
}