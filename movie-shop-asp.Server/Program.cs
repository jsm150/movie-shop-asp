using movie_shop_asp.Server.Extensions;
using Movie.API.Controllers;
using Scalar.AspNetCore;
using Screening.API.Controllers;
using Theater.API.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.AddApplicationServices();

builder.Services.AddControllers()
    .AddApplicationPart(typeof(MovieController).Assembly)
    .AddApplicationPart(typeof(ScreenController).Assembly)
    .AddApplicationPart(typeof(TheaterController).Assembly);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler(options => { });

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
