using BikeStore.Web.Models;
using BikeStore.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BikeStore.Web.Controllers;

public sealed class HomeController : Controller
{
    private readonly IBicycleApiClient _apiClient;

    public HomeController(IBicycleApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        try
        {
            var bicycles =
                await _apiClient.GetBicyclesAsync(
                    cancellationToken: cancellationToken);

            var model = new DashboardViewModel
            {
                TotalBicycles = bicycles.Count,

                AvailableBicycles = bicycles.Count(
                    bicycle => IsStatus(
                        bicycle,
                        "Disponible")),

                LowStockBicycles = bicycles.Count(
                    bicycle => IsStatus(
                        bicycle,
                        "Bajo stock")),

                OutOfStockBicycles = bicycles.Count(
                    bicycle => IsStatus(
                        bicycle,
                        "Agotado")),

                InventoryValue = bicycles.Sum(
                    bicycle => bicycle.Price * bicycle.Stock),

                BicyclesToWatch = bicycles
                    .OrderBy(bicycle => bicycle.Stock)
                    .ThenBy(bicycle => bicycle.Brand)
                    .ThenBy(bicycle => bicycle.Model)
                    .Take(4)
                    .ToList()
            };

            return View(model);
        }
        catch (BicycleApiException exception)
        {
            return View(
                new DashboardViewModel
                {
                    ErrorMessage = exception.Message
                });
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(
            new ErrorViewModel
            {
                RequestId =
                    Activity.Current?.Id ??
                    HttpContext.TraceIdentifier
            });
    }

    private static bool IsStatus(
        BicycleViewModel bicycle,
        string status)
    {
        return string.Equals(
            bicycle.Status,
            status,
            StringComparison.OrdinalIgnoreCase);
    }
}