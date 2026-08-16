using BikeStore.Application.DTOs;
using BikeStore.Domain.Entities;

namespace BikeStore.Application.Contracts;

public sealed record BicycleWithCategory(
    Bicycle Entity,
    string Category);

public interface IBicycleRepository
{
    Task<IReadOnlyList<BicycleWithCategory>> GetAllAsync(
        BicycleFilter filter,
        CancellationToken cancellationToken);

    Task<BicycleWithCategory?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CategoryLookupDto>> GetActiveCategoriesAsync(
        CancellationToken cancellationToken);

    Task<bool> ActiveCategoryExistsAsync(
        int categoryId,
        CancellationToken cancellationToken);

    void Add(Bicycle bicycle);

    void Remove(Bicycle bicycle);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}