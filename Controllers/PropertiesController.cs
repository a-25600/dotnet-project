using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApi.Data;
using RealEstateApi.DTOs;
using RealEstateApi.Models;

namespace RealEstateApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertiesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PropertiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProperties([FromQuery] PropertyQueryParameters parameters)
    {
        var query = _context.Properties.AsQueryable();

        if (parameters.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= parameters.MaxPrice.Value);

        if (parameters.MinRooms.HasValue)
            query = query.Where(p => p.RoomsCount >= parameters.MinRooms.Value);

        if (parameters.IsAvailable.HasValue)
            query = query.Where(p => p.IsAvailable == parameters.IsAvailable.Value);

        var properties = await query.ToListAsync();
        return Ok(properties);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProperty(int id)
    {
        var property = await _context.Properties.FindAsync(id);

        if (property == null)
            return NotFound();

        return Ok(property);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProperty([FromBody] PropertyDto dto)
    {
        var newProperty = new Property
        {
            Address = dto.Address,
            Price = dto.Price,
            RoomsCount = dto.RoomsCount,
            IsAvailable = dto.IsAvailable
        };

        await _context.Properties.AddAsync(newProperty);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProperty), new { id = newProperty.Id }, newProperty);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProperty(int id, [FromBody] PropertyDto dto)
    {
        var existingProperty = await _context.Properties.FindAsync(id);

        if (existingProperty == null)
            return NotFound();

        existingProperty.Address = dto.Address;
        existingProperty.Price = dto.Price;
        existingProperty.RoomsCount = dto.RoomsCount;
        existingProperty.IsAvailable = dto.IsAvailable;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProperty(int id)
    {
        var property = await _context.Properties.FindAsync(id);

        if (property == null)
            return NotFound();

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}