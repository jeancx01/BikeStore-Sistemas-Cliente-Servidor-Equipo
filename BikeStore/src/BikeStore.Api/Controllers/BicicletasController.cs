using BikeStore.Application.DTOs;
using BikeStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BikeStore.Api.Controllers;

[ApiController]
[Route("api/bicicletas")]
public sealed class BicicletasController(
    IBicycleService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<BicycleDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<
        IReadOnlyList<BicycleDto>>> GetAll(
            [FromQuery] BicycleFilter filter,
            CancellationToken cancellationToken)
    {
        var bicycles = await service.GetAllAsync(
            filter,
            cancellationToken);

        return Ok(bicycles);
    }

    [HttpGet("stock-bajo")]
    [ProducesResponseType(
        typeof(IReadOnlyList<BicycleDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<
        IReadOnlyList<BicycleDto>>> GetLowStock(
            CancellationToken cancellationToken)
    {
        var bicycles = await service.GetAllAsync(
            new BicycleFilter
            {
                LowStock = true
            },
            cancellationToken);

        return Ok(bicycles);
    }

    [HttpGet("agotadas")]
    [ProducesResponseType(
        typeof(IReadOnlyList<BicycleDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<
        IReadOnlyList<BicycleDto>>> GetOutOfStock(
            CancellationToken cancellationToken)
    {
        var bicycles = await service.GetAllAsync(
            new BicycleFilter
            {
                OutOfStock = true
            },
            cancellationToken);

        return Ok(bicycles);
    }

    [HttpGet("categorias")]
    [ProducesResponseType(
        typeof(IReadOnlyList<CategoryLookupDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<
        IReadOnlyList<CategoryLookupDto>>> GetCategories(
            CancellationToken cancellationToken)
    {
        var categories =
            await service.GetCategoriesAsync(
                cancellationToken);

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(BicycleDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BicycleDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var bicycle = await service.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(bicycle);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(BicycleDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BicycleDto>> Create(
        [FromBody] SaveBicycleRequest request,
        CancellationToken cancellationToken)
    {
        var bicycle = await service.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = bicycle.Id },
            bicycle);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] SaveBicycleRequest request,
        CancellationToken cancellationToken)
    {
        await service.UpdateAsync(
            id,
            request,
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        await service.DeleteAsync(
            id,
            cancellationToken);

        return NoContent();
    }
}