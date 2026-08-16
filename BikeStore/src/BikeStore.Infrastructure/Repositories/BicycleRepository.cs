using System.Data;
using BikeStore.Application.Common;
using BikeStore.Application.Contracts;
using BikeStore.Application.DTOs;
using BikeStore.Domain.Entities;
using BikeStore.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BikeStore.Infrastructure.Repositories;

public sealed class BicycleRepository(
    BikeStoreDbContext context) : IBicycleRepository
{
    public async Task<IReadOnlyList<BicycleWithCategory>> GetAllAsync(
        BicycleFilter filter,
        CancellationToken cancellationToken)
    {
        var query = context.Bicycles
            .AsNoTracking()
            .AsQueryable();

        var name = filter.Name?.Trim();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x =>
                x.Brand.Contains(name) ||
                x.Model.Contains(name));
        }

        if (filter.CategoryId is > 0)
        {
            query = query.Where(x =>
                x.CategoryId == filter.CategoryId);
        }

        var brand = filter.Brand?.Trim();

        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(x =>
                x.Brand.Contains(brand));
        }

        var threshold = Math.Max(
            1,
            filter.LowStockThreshold);

        if (filter.LowStock && filter.OutOfStock)
        {
            query = query.Where(x =>
                x.Stock <= threshold);
        }
        else if (filter.OutOfStock)
        {
            query = query.Where(x =>
                x.Stock == 0);
        }
        else if (filter.LowStock)
        {
            query = query.Where(x =>
                x.Stock > 0 &&
                x.Stock <= threshold);
        }

        var bicycles = await query
            .OrderBy(x => x.Brand)
            .ThenBy(x => x.Model)
            .ToListAsync(cancellationToken);

        var categories = await ReadCategoriesAsync(
            onlyActive: false,
            cancellationToken);

        var categoryNames = categories.ToDictionary(
            x => x.Id,
            x => x.Name);

        return bicycles.Select(bicycle =>
        {
            var category = categoryNames.TryGetValue(
                bicycle.CategoryId,
                out var name)
                    ? name
                    : "Sin categoría";

            return new BicycleWithCategory(
                bicycle,
                category);
        }).ToList();
    }

    public async Task<BicycleWithCategory?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var bicycle = await context.Bicycles
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (bicycle is null)
        {
            return null;
        }

        var categories = await ReadCategoriesAsync(
            onlyActive: false,
            cancellationToken);

        var category = categories
            .FirstOrDefault(x =>
                x.Id == bicycle.CategoryId)
            ?.Name ?? "Sin categoría";

        return new BicycleWithCategory(
            bicycle,
            category);
    }

    public Task<IReadOnlyList<CategoryLookupDto>>
        GetActiveCategoriesAsync(
            CancellationToken cancellationToken)
    {
        return ReadCategoriesAsync(
            onlyActive: true,
            cancellationToken);
    }

    public async Task<bool> ActiveCategoryExistsAsync(
        int categoryId,
        CancellationToken cancellationToken)
    {
        var connection =
            context.Database.GetDbConnection();

        var shouldClose =
            connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync(
                cancellationToken);
        }

        try
        {
            await using var command =
                connection.CreateCommand();

            command.CommandText = """
                SELECT CASE
                    WHEN EXISTS
                    (
                        SELECT 1
                        FROM dbo.Categoria
                        WHERE IdCategoria = @categoryId
                          AND Activo = 1
                    )
                    THEN CAST(1 AS bit)
                    ELSE CAST(0 AS bit)
                END;
                """;

            var parameter =
                command.CreateParameter();

            parameter.ParameterName = "@categoryId";
            parameter.DbType = DbType.Int32;
            parameter.Value = categoryId;

            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync(
                cancellationToken);

            return result is not null &&
                   Convert.ToBoolean(result);
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }

    public void Add(Bicycle bicycle)
    {
        context.Bicycles.Add(bicycle);
    }

    public void Remove(Bicycle bicycle)
    {
        context.Bicycles.Remove(bicycle);
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return await context.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException
                is SqlException { Number: 547 })
        {
            throw new ConflictException(
                "No se puede eliminar la bicicleta porque está relacionada con una venta.");
        }
    }

    private async Task<IReadOnlyList<CategoryLookupDto>>
        ReadCategoriesAsync(
            bool onlyActive,
            CancellationToken cancellationToken)
    {
        var connection =
            context.Database.GetDbConnection();

        var shouldClose =
            connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync(
                cancellationToken);
        }

        try
        {
            await using var command =
                connection.CreateCommand();

            command.CommandText = onlyActive
                ? """
                  SELECT IdCategoria, Nombre
                  FROM dbo.Categoria
                  WHERE Activo = 1
                  ORDER BY Nombre;
                  """
                : """
                  SELECT IdCategoria, Nombre
                  FROM dbo.Categoria
                  ORDER BY Nombre;
                  """;

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            var categories =
                new List<CategoryLookupDto>();

            while (await reader.ReadAsync(
                cancellationToken))
            {
                categories.Add(
                    new CategoryLookupDto(
                        reader.GetInt32(0),
                        reader.GetString(1)));
            }

            return categories;
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}
