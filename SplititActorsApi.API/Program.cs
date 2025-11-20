using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using SplititActorsApi.API.Authentication;
using SplititActorsApi.API.Middleware;
using SplititActorsApi.Business.Services;
using SplititActorsApi.Core.Interfaces;
using SplititActorsApi.Data.DbContexts;
using SplititActorsApi.Data.Repositories;
using SplititActorsApi.Data.Scrapers;
using SplititActorsApi.Data.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Authentication - Support Splitit's sandbox API key format
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });

builder.Services.AddAuthorization();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure HttpClient for scraper
builder.Services.AddHttpClient<IScraperProvider, ImdbScraperProvider>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
});

// Configure In-Memory Database
builder.Services.AddDbContext<ActorsDbContext>(options =>
    options.UseInMemoryDatabase("ActorsDb"));

// Register repositories
builder.Services.AddScoped<IActorRepository, ActorRepository>();

// Register business services
builder.Services.AddScoped<IActorService, ActorService>();

// Register data seeder
builder.Services.AddScoped<DataSeeder>();

var app = builder.Build();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
// Add global exception handler
app.UseMiddleware<GlobalExceptionHandler>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Splitit Actors API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at root
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
