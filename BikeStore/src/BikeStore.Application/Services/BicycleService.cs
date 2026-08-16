using BikeStore.Application.Common;
using BikeStore.Application.Contracts;
using BikeStore.Application.DTOs;
using BikeStore.Application.Interfaces;
using BikeStore.Domain.Entities;
using BikeStore.Domain.Enums;

namespace BikeStore.Application.Services;

public sealed class BicycleService(
    IBicycleRepository repository,
    BusinessOptions options) : IBicycleService
{
    public async Task<IReadOnlyList<BicycleDto>> GetAllAsync(
        BicycleFilter filter,
        CancellationToken cancellationToken)
    {
        var bicycles = await repository.GetAllAsync(
            filter,
            cancellationToken);

        return bicycles.Select(Map).ToList();
    }

    public async Task<BicycleDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return Map(await FindAsync(id, cancellationToken));
    }

    public Task<IReadOnlyList<CategoryLookupDto>> GetCategoriesAsync(
        CancellationToken cancellationToken)
    {
        return repository.GetActiveCategoriesAsync(cancellationToken);
    }

    public async Task<BicycleDto> CreateAsync(
        SaveBicycleRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);

        await ValidateCategoryAsync(
            request.CategoryId,
            cancellationToken);

        var bicycle = new Bicycle();

        Apply(bicycle, request);

        repository.Add(bicycle);

        await repository.SaveChangesAsync(cancellationToken);

        return Map(await FindAsync(
            bicycle.Id,
            cancellationToken));
    }

    public async Task UpdateAsync(
        int id,
        SaveBicycleRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);

        var item = await FindAsync(id, cancellationToken);

        await ValidateCategoryAsync(
            request.CategoryId,
            cancellationToken);

        Apply(item.Entity, request);

        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var item = await FindAsync(id, cancellationToken);

        repository.Remove(item.Entity);

        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<BicycleWithCategory> FindAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(
                "La bicicleta solicitada no fue encontrada.");
    }

    private async Task ValidateCategoryAsync(
        int categoryId,
        CancellationToken cancellationToken)
    {
        var exists = await repository.ActiveCategoryExistsAsync(
            categoryId,
            cancellationToken);

        if (!exists)
        {
            throw new BusinessException(
                "La categoría seleccionada no existe o está inactiva.");
        }
    }

    private static void Validate(SaveBicycleRequest request)
    {
        if (request.CategoryId <= 0)
        {
            throw new BusinessException(
                "Seleccione una categoría válida.");
        }

        ValidateText(
            request.Brand,
            2,
            100,
            "La marca debe tener entre 2 y 100 caracteres.");

        ValidateText(
            request.Model,
            1,
            100,
            "El modelo debe tener entre 1 y 100 caracteres.");

        if (request.Price < 0.01m ||
            request.Price > 9999999.99m)
        {
            throw new BusinessException(
                "El precio debe estar entre 0,01 y 9 999 999,99.");
        }

        if (request.Stock < 0 ||
            request.Stock > 100000)
        {
            throw new BusinessException(
                "El stock debe estar entre 0 y 100 000.");
        }
    }

    private static void ValidateText(
        string? value,
        int minimum,
        int maximum,
        string message)
    {
        var length = value?.Trim().Length ?? 0;

        if (length < minimum || length > maximum)
        {
            throw new BusinessException(message);
        }
    }

    private void Apply(
        Bicycle bicycle,
        SaveBicycleRequest request)
    {
        bicycle.CategoryId = request.CategoryId;
        bicycle.Brand = request.Brand.Trim();
        bicycle.Model = request.Model.Trim();
        bicycle.Price = decimal.Round(request.Price, 2);
        bicycle.Stock = request.Stock;
        bicycle.Status = GetStatus(request.Stock);
    }

    private BicycleStatus GetStatus(int stock)
    {
        if (stock == 0)
        {
            return BicycleStatus.Agotado;
        }

        if (stock <= options.LowStockThreshold)
        {
            return BicycleStatus.BajoStock;
        }

        return BicycleStatus.Disponible;
    }

    private static BicycleDto Map(
        BicycleWithCategory item)
    {
        return new BicycleDto(
            item.Entity.Id,
            item.Entity.CategoryId,
            item.Category,
            item.Entity.Brand,
            item.Entity.Model,
            item.Entity.Price,
            item.Entity.Stock,
            StatusText(item.Entity.Status));
    }

    private static string StatusText(BicycleStatus status)
    {
        return status switch
        {
            BicycleStatus.BajoStock => "Bajo stock",
            BicycleStatus.Agotado => "Agotado",
            BicycleStatus.Inactivo => "Inactivo",
            _ => "Disponible"
        };
    }
}