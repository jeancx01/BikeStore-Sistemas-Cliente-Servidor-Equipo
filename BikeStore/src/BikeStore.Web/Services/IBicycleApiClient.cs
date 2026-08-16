using BikeStore.Web.Models;

namespace BikeStore.Web.Services;

public interface IBicycleApiClient
{
    Task<IReadOnlyList<BicycleViewModel>> GetBicyclesAsync(
        string? name = null,
        int? categoryId = null,
        string? brand = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BicycleViewModel>> GetLowStockAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BicycleViewModel>> GetOutOfStockAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryOptionViewModel>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);

    Task<BicycleViewModel> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<BicycleViewModel> CreateAsync(
        BicycleFormViewModel model,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        int id,
        BicycleFormViewModel model,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}