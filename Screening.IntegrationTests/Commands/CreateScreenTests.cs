using BuildingBlocks.IntegrationTest.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using movie_shop_asp.Server.Infrastructure;
using Npgsql;
using Screening.Domain.Aggregate.MovieAggregate;
using Screening.Domain.Aggregate.TheaterAggregate;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using TheaterEntity = Screening.Domain.Aggregate.TheaterAggregate.Theater;


namespace Screening.IntegrationTests.Commands;

[Collection(nameof(IntegrationTestCollection))]
public class CreateScreenTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateScreen_WithValidData_ReturnsScreenId()
    {
        // Arrange
        var (movieId, theaterId) = await SeedTestDataAsync();

        var command = new
        {
            MovieId = movieId,
            TheaterId = theaterId,
            StartTime = DateTimeOffset.UtcNow.AddDays(1),
            EndTime = DateTimeOffset.UtcNow.AddDays(1).AddHours(2),
            SalesStartAt = DateTimeOffset.UtcNow,
            SalesEndAt = DateTimeOffset.UtcNow.AddDays(1).AddHours(-1)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/screen", command);

        // Assert
        response.EnsureSuccessStatusCode();
        var screenId = await response.Content.ReadFromJsonAsync<long>();
        Assert.True(screenId > 0);
    }

    [Fact]
    public async Task CreateScreen_WithNonExistentMovie_ReturnsBadRequest()
    {
        // Arrange
        var (_, theaterId) = await SeedTestDataAsync();

        var command = new
        {
            MovieId = 99999L,
            TheaterId = theaterId,
            StartTime = DateTimeOffset.UtcNow.AddDays(1),
            EndTime = DateTimeOffset.UtcNow.AddDays(1).AddHours(2),
            SalesStartAt = DateTimeOffset.UtcNow,
            SalesEndAt = DateTimeOffset.UtcNow.AddDays(1).AddHours(-1)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/screen", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateScreen_WithNonExistentTheater_ReturnsBadRequest()
    {
        // Arrange
        var (movieId, _) = await SeedTestDataAsync();

        var command = new
        {
            MovieId = movieId,
            TheaterId = 99999L,
            StartTime = DateTimeOffset.UtcNow.AddDays(1),
            EndTime = DateTimeOffset.UtcNow.AddDays(1).AddHours(2),
            SalesStartAt = DateTimeOffset.UtcNow,
            SalesEndAt = DateTimeOffset.UtcNow.AddDays(1).AddHours(-1)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/screen", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateScreen_WithConflictingSchedule_ReturnsBadRequest()
    {
        // Arrange
        var (movieId, theaterId) = await SeedTestDataAsync();

        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(2);

        var firstCommand = new
        {
            MovieId = movieId,
            TheaterId = theaterId,
            StartTime = startTime,
            EndTime = endTime,
            SalesStartAt = DateTimeOffset.UtcNow,
            SalesEndAt = startTime.AddHours(-1)
        };

        // 첫 번째 상영 생성
        var firstResponse = await Client.PostAsJsonAsync("/api/screen", firstCommand);
        firstResponse.EnsureSuccessStatusCode();

        // 겹치는 시간대로 두 번째 상영 시도
        var conflictingCommand = new
        {
            MovieId = movieId,
            TheaterId = theaterId,
            StartTime = startTime.AddHours(1), // 첫 번째 상영과 겹침
            EndTime = endTime.AddHours(1),
            SalesStartAt = DateTimeOffset.UtcNow,
            SalesEndAt = startTime.AddHours(1).AddMinutes(-30)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/screen", conflictingCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateScreen_WithNonOverlappingSchedule_Succeeds()
    {
        // Arrange
        var (movieId, theaterId) = await SeedTestDataAsync();

        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(2);

        var firstCommand = new
        {
            MovieId = movieId,
            TheaterId = theaterId,
            StartTime = startTime,
            EndTime = endTime,
            SalesStartAt = DateTimeOffset.UtcNow,
            SalesEndAt = startTime.AddHours(-1)
        };

        await Client.PostAsJsonAsync("/api/screen", firstCommand);

        // 겹치지 않는 시간대로 두 번째 상영
        var secondCommand = new
        {
            MovieId = movieId,
            TheaterId = theaterId,
            StartTime = endTime.AddMinutes(30), // 첫 번째 상영 후
            EndTime = endTime.AddHours(2),
            SalesStartAt = DateTimeOffset.UtcNow,
            SalesEndAt = endTime.AddMinutes(15)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/screen", secondCommand);

        // Assert
        response.EnsureSuccessStatusCode();
        var screenId = await response.Content.ReadFromJsonAsync<long>();
        Assert.True(screenId > 0);
    }

    /// <summary>
    /// DB 레벨의 exclusion constraint(no_overlap_screening)가 
    /// 상영 시간 중복을 방지하는지 검증합니다.
    /// 애플리케이션 레벨 검증을 우회하여 직접 DbContext로 삽입을 시도합니다.
    /// </summary>
    [Fact]
    public async Task CreateScreen_DbLevelConstraint_PreventsOverlappingSchedule()
    {
        // Arrange
        var (movieId, theaterId) = await SeedTestDataAsync();

        var startTime = DateTimeOffset.UtcNow.AddDays(10);
        var endTime = startTime.AddHours(2);

        // 첫 번째 상영을 직접 DB에 삽입
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MovieShopContext>();

            await context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""Screening"".""Screens"" 
                    (""MovieId"", ""TheaterId"", ""StartTime"", ""EndTime"", ""SalesStartAt"", ""SalesEndAt"", ""Status"")
                VALUES 
                    ({movieId}, {theaterId}, {startTime}, {endTime}, {startTime.AddDays(-1)}, {startTime.AddHours(-1)}, 0)
            ");
        }

        // Act & Assert - 겹치는 시간대로 두 번째 상영 직접 삽입 시도
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MovieShopContext>();

            var overlappingStartTime = startTime.AddHours(1); // 첫 번째 상영과 겹침
            var overlappingEndTime = endTime.AddHours(1);

            // PostgreSQL exclusion constraint 위반 시 PostgresException 발생
            var exception = await Assert.ThrowsAsync<PostgresException>(async () =>
            {
                await context.Database.ExecuteSqlInterpolatedAsync($@"
                    INSERT INTO ""Screening"".""Screens"" 
                        (""MovieId"", ""TheaterId"", ""StartTime"", ""EndTime"", ""SalesStartAt"", ""SalesEndAt"", ""Status"")
                    VALUES 
                        ({movieId}, {theaterId}, {overlappingStartTime}, {overlappingEndTime}, {overlappingStartTime.AddDays(-1)}, {overlappingStartTime.AddHours(-1)}, 0)
                ");
            });

            // exclusion constraint 위반 에러 코드: 23P01
            Assert.Equal("23P01", exception.SqlState);
            Assert.Contains("no_overlap_screening", exception.Message);
        }
    }

    /// <summary>
    /// DB 레벨 constraint가 종료 시간과 시작 시간이 정확히 일치하는 경우(경계값)를
    /// 허용하는지 검증합니다. (tstzrange의 '[)' 옵션: 시작 포함, 종료 미포함)
    /// </summary>
    [Fact]
    public async Task CreateScreen_DbLevelConstraint_AllowsAdjacentSchedule()
    {
        // Arrange
        var (movieId, theaterId) = await SeedTestDataAsync();

        var firstStartTime = DateTimeOffset.UtcNow.AddDays(20);
        var firstEndTime = firstStartTime.AddHours(2);

        // 첫 번째 상영을 직접 DB에 삽입
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MovieShopContext>();

            await context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""Screening"".""Screens"" 
                    (""MovieId"", ""TheaterId"", ""StartTime"", ""EndTime"", ""SalesStartAt"", ""SalesEndAt"", ""Status"")
                VALUES 
                    ({movieId}, {theaterId}, {firstStartTime}, {firstEndTime}, {firstStartTime.AddDays(-1)}, {firstStartTime.AddHours(-1)}, 0)
            ");
        }

        // Act - 첫 번째 상영 종료 시간에 정확히 시작하는 두 번째 상영 삽입
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MovieShopContext>();

            var secondStartTime = firstEndTime; // 정확히 첫 번째 종료 시간에 시작
            var secondEndTime = secondStartTime.AddHours(2);

            // 예외 없이 성공해야 함 (경계값 허용)
            await context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO ""Screening"".""Screens"" 
                    (""MovieId"", ""TheaterId"", ""StartTime"", ""EndTime"", ""SalesStartAt"", ""SalesEndAt"", ""Status"")
                VALUES 
                    ({movieId}, {theaterId}, {secondStartTime}, {secondEndTime}, {secondStartTime.AddDays(-1)}, {secondStartTime.AddHours(-1)}, 0)
            ");
        }

        // Assert - 두 개의 상영이 모두 저장되었는지 확인
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MovieShopContext>();
            var screenCount = await context.Screens
                .CountAsync(s => s.TheaterId == theaterId && s.StartTime >= firstStartTime);

            Assert.Equal(2, screenCount);
        }
    }

    private async Task<(long MovieId, long TheaterId)> SeedTestDataAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieShopContext>();

        var movie = new Domain.Aggregate.MovieAggregate.Movie
        {
            MovieId = 1,
            MovieStatus = MovieStatus.NOW_SHOWING
        };
        context.Set<Domain.Aggregate.MovieAggregate.Movie>().Add(movie);

        var seatCodes = new[] { new SeatCode("A1"), new SeatCode("A2"), new SeatCode("A3") };
        var theater = new TheaterEntity(1, seatCodes);
        context.Set<TheaterEntity>().Add(theater);

        await context.SaveChangesAsync();

        return (movie.MovieId, theater.TheaterId);
    }
}