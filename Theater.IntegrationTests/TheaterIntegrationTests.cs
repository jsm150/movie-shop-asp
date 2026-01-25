using BuildingBlocks.IntegrationTest.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using movie_shop_asp.Server.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Theater.API.Application.Commands;
using Theater.Domain.Aggregate;

namespace Theater.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class TheaterIntegrationTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateTheater_WithValidData_ShouldPersistToDatabase()
    {
        // Arrange
        var command = new CreateTheaterCommand
        {
            Name = "IMAX관",
            Floor = 2,
            Type = TheaterType.IMAX,
            RowCount = 2,
            ColumnCount = 2,
            Seats = ["A1", "A2", "B1", "B2"]
        };

        // Act
        var theaterId = await PostAsync<CreateTheaterCommand, long>("/api/theater", command);

        // Assert - Theater 모듈
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MovieShopContext>();
        
        var theater = await context.Theaters
            .Include(t => t.Seats)
            .FirstOrDefaultAsync(t => t.TheaterId == theaterId);

        theater.Should().NotBeNull();
        theater!.Name.Should().Be("IMAX관");
        theater.Floor.Should().Be(2);
        theater.Type.Should().Be(TheaterType.IMAX);
        theater.RowCount.Should().Be(2);
        theater.ColumnCount.Should().Be(2);
        theater.Seats.Should().HaveCount(4);
        theater.IsActive.Should().BeTrue();

        // Assert - Screening 모듈 (통합 이벤트로 생성된 Theater)
        var screeningTheater = await context.ScreeingTheaters
            .Include(t => t.Seats)
            .FirstOrDefaultAsync(t => t.TheaterId == theaterId);

        screeningTheater.Should().NotBeNull();
        screeningTheater!.TheaterId.Should().Be(theaterId);
        screeningTheater.Seats.Should().HaveCount(4);
        screeningTheater.Seats.Select(s => s.SeatCode.Value)
            .Should().BeEquivalentTo(["A1", "A2", "B1", "B2"]);
        screeningTheater.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateTheater_WithDuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateTheaterCommand
        {
            Name = "중복테스트관",
            Floor = 1,
            Type = TheaterType.Standard,
            RowCount = 1,
            ColumnCount = 1,
            Seats = ["A1"]
        };

        await PostAsync<CreateTheaterCommand, long>("/api/theater", command);

        // Act
        var response = await Client.PostAsJsonAsync("/api/theater", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
}