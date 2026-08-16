namespace BikeStore.Web.Models;

public sealed class DashboardViewModel
{
    public int TotalBicycles { get; init; }

    public int AvailableBicycles { get; init; }

    public int LowStockBicycles { get; init; }

    public int OutOfStockBicycles { get; init; }

    public decimal InventoryValue { get; init; }

    public IReadOnlyList<BicycleViewModel> BicyclesToWatch { get; init; } = [];

    public bool HasBicycles => TotalBicycles > 0;

    public string? ErrorMessage { get; init; }
}