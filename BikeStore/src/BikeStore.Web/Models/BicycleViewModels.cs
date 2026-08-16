using System.ComponentModel.DataAnnotations;

namespace BikeStore.Web.Models;

public sealed class BicycleViewModel
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public string Status { get; set; } = string.Empty;

    public string DisplayName => $"{Brand} {Model}";
}

public sealed class CategoryOptionViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public sealed class BicycleFormViewModel
{
    public int Id { get; set; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Seleccione una categoría válida.")]
    [Display(Name = "Categoría")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "La marca es obligatoria.")]
    [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage = "La marca debe tener entre 2 y 100 caracteres.")]
    [Display(Name = "Marca")]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "El modelo es obligatorio.")]
    [StringLength(
        100,
        MinimumLength = 1,
        ErrorMessage = "El modelo debe tener entre 1 y 100 caracteres.")]
    [Display(Name = "Modelo")]
    public string Model { get; set; } = string.Empty;

    [Range(
        typeof(decimal),
        "0.01",
        "9999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "El precio debe ser mayor que cero.")]
    [Display(Name = "Precio")]
    public decimal Price { get; set; }

    [Range(
        0,
        100000,
        ErrorMessage = "El stock debe estar entre 0 y 100 000.")]
    [Display(Name = "Stock")]
    public int Stock { get; set; }
}

public sealed class BicycleIndexViewModel
{
    public IReadOnlyList<BicycleViewModel> Items { get; init; } = [];

    public IReadOnlyList<CategoryOptionViewModel> Categories
    {
        get;
        init;
    } = [];

    public string? Search { get; init; }

    public int? CategoryId { get; init; }

    public string? Brand { get; init; }

    public string? Status { get; init; }

    public int Total { get; init; }

    public int Available { get; init; }

    public int LowStock { get; init; }

    public int OutOfStock { get; init; }
}