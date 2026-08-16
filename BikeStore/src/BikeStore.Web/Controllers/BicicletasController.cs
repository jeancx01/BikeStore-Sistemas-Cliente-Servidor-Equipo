using BikeStore.Web.Models;
using BikeStore.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BikeStore.Web.Controllers;

public sealed class BicicletasController : Controller
{
    private readonly IBicycleApiClient _apiClient;

    public BicicletasController(
        IBicycleApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? buscar,
        int? categoriaId,
        string? marca,
        string? estado,
        CancellationToken cancellationToken)
    {
        try
        {
            var allBicycles =
                await _apiClient.GetBicyclesAsync(
                    cancellationToken: cancellationToken);

            var filteredBicycles =
                await _apiClient.GetBicyclesAsync(
                    buscar,
                    categoriaId,
                    marca,
                    cancellationToken);

            filteredBicycles =
                FilterByStatus(
                    filteredBicycles,
                    estado);

            var categories =
                await _apiClient.GetCategoriesAsync(
                    cancellationToken);

            var model = new BicycleIndexViewModel
            {
                Items = filteredBicycles,
                Categories = categories,

                Search = buscar,
                CategoryId = categoriaId,
                Brand = marca,
                Status = estado,

                Total = allBicycles.Count,

                Available = allBicycles.Count(
                    bicycle => IsStatus(
                        bicycle,
                        "Disponible")),

                LowStock = allBicycles.Count(
                    bicycle => IsStatus(
                        bicycle,
                        "Bajo stock")),

                OutOfStock = allBicycles.Count(
                    bicycle => IsStatus(
                        bicycle,
                        "Agotado"))
            };

            return View(model);
        }
        catch (BicycleApiException exception)
        {
            ViewData["ApiError"] =
                exception.Message;

            return View(
                new BicycleIndexViewModel
                {
                    Search = buscar,
                    CategoryId = categoriaId,
                    Brand = marca,
                    Status = estado
                });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Crear(
        CancellationToken cancellationToken)
    {
        try
        {
            await LoadCategoriesAsync(
                null,
                cancellationToken);

            return View(
                "Formulario",
                new BicycleFormViewModel());
        }
        catch (BicycleApiException exception)
        {
            TempData["Error"] =
                exception.Message;

            return RedirectToAction(
                nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(
    BicycleFormViewModel form,
    CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(
                form.CategoryId,
                cancellationToken);

            return View(
                "Formulario",
                form);
        }

        try
        {
            var created =
                await _apiClient.CreateAsync(
                    form,
                    cancellationToken);

            TempData["Success"] =
                $"La bicicleta {created.DisplayName} " +
                "fue registrada correctamente.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (BicycleApiException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            await LoadCategoriesAsync(
                form.CategoryId,
                cancellationToken);

            return View(
                "Formulario",
                form);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Editar(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var bicycle =
                await _apiClient.GetByIdAsync(
                    id,
                    cancellationToken);

            await LoadCategoriesAsync(
                bicycle.CategoryId,
                cancellationToken);

            var model =
                new BicycleFormViewModel
                {
                    Id = bicycle.Id,
                    CategoryId =
                        bicycle.CategoryId,
                    Brand = bicycle.Brand,
                    Model = bicycle.Model,
                    Price = bicycle.Price,
                    Stock = bicycle.Stock
                };

            return View(
                "Formulario",
                model);
        }
        catch (BicycleApiException exception)
        {
            TempData["Error"] =
                exception.Message;

            return RedirectToAction(
                nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(
    BicycleFormViewModel form,
    CancellationToken cancellationToken)
    {
        if (form.Id <= 0)
        {
            ModelState.AddModelError(
                nameof(form.Id),
                "El identificador no es válido.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(
                form.CategoryId,
                cancellationToken);

            return View(
                "Formulario",
                form);
        }

        try
        {
            await _apiClient.UpdateAsync(
                form.Id,
                form,
                cancellationToken);

            TempData["Success"] =
                "La bicicleta fue actualizada correctamente.";

            return RedirectToAction(
                nameof(Index));
        }
        catch (BicycleApiException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            await LoadCategoriesAsync(
                form.CategoryId,
                cancellationToken);

            return View(
                "Formulario",
                form);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _apiClient.DeleteAsync(
                id,
                cancellationToken);

            TempData["Success"] =
                "La bicicleta fue eliminada correctamente.";
        }
        catch (BicycleApiException exception)
        {
            TempData["Error"] =
                exception.Message;
        }

        return RedirectToAction(
            nameof(Index));
    }

    private async Task LoadCategoriesAsync(
        int? selectedCategoryId,
        CancellationToken cancellationToken)
    {
        var categories =
            await _apiClient.GetCategoriesAsync(
                cancellationToken);

        ViewBag.Categories =
            new SelectList(
                categories,
                nameof(CategoryOptionViewModel.Id),
                nameof(CategoryOptionViewModel.Name),
                selectedCategoryId);
    }

    private static IReadOnlyList<BicycleViewModel>
        FilterByStatus(
            IReadOnlyList<BicycleViewModel> bicycles,
            string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return bicycles;
        }

        var expectedStatus =
            status.ToLowerInvariant() switch
            {
                "disponibles" => "Disponible",
                "stock-bajo" => "Bajo stock",
                "agotadas" => "Agotado",
                _ => string.Empty
            };

        if (string.IsNullOrEmpty(expectedStatus))
        {
            return bicycles;
        }

        return bicycles
            .Where(
                bicycle => IsStatus(
                    bicycle,
                    expectedStatus))
            .ToList();
    }

    private static bool IsStatus(
        BicycleViewModel bicycle,
        string expectedStatus)
    {
        return string.Equals(
            bicycle.Status,
            expectedStatus,
            StringComparison.OrdinalIgnoreCase);
    }
}