using BikeStore.Api.Middleware;
using BikeStore.Application.Common;
using BikeStore.Application.Contracts;
using BikeStore.Application.Interfaces;
using BikeStore.Application.Services;
using BikeStore.Infrastructure.Data;
using BikeStore.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString =
    builder.Configuration.GetConnectionString("BikeStoreDb")
    ?? throw new InvalidOperationException(
        "No se encontró la conexión BikeStoreDb.");

builder.Services.AddDbContext<BikeStoreDbContext>(
    options =>
        options.UseSqlServer(connectionString));

builder.Services.AddScoped<
    IBicycleRepository,
    BicycleRepository>();

builder.Services.AddScoped<
    IBicycleService,
    BicycleService>();

builder.Services.AddSingleton(
    new BusinessOptions
    {
        LowStockThreshold =
            builder.Configuration.GetValue(
                "BusinessOptions:LowStockThreshold",
                5)
    });

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<BikeStoreDbContext>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();