using Microsoft.AspNetCore.Mvc;
using RealEstateApi.Data;
using RealEstateApi.DTOs;
using RealEstateApi.Models;
using System.Linq;

namespace RealEstateApi.Controllers;

[ApiController]
[Route("api/[controller]")] // Routing: /api/properties
public class PropertiesController : ControllerBase
{
    // GET: api/properties?maxPrice=100000&minRooms=2
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetProperties([FromQuery] PropertyQueryParameters parameters) // Model binding для Query
    {
        // Звертаємось безпосередньо до статичного списку
        var query = MockDatabase.Properties.AsEnumerable();

        if (parameters.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= parameters.MaxPrice.Value);

        if (parameters.MinRooms.HasValue)
            query = query.Where(p => p.RoomsCount >= parameters.MinRooms.Value);

        if (parameters.IsAvailable.HasValue)
            query = query.Where(p => p.IsAvailable == parameters.IsAvailable.Value);

        return Ok(query.ToList()); // 200 OK
    }

    // GET: api/properties/1
    [HttpGet("{id}")] // Route parameter
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetProperty(int id)
    {
        // Шукаємо об'єкт у звичайному списку
        var property = MockDatabase.Properties.FirstOrDefault(p => p.Id == id);

        if (property != null)
            return Ok(property); // 200 OK

        return NotFound(new { Message = $"Об'єкт з ID {id} не знайдено." }); // 404 Not Found
    }

    // POST: api/properties
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateProperty([FromBody] PropertyDto dto) // Model binding для Body
    {
        // Генеруємо новий ID (якщо список порожній, ставимо 1)
        var newId = MockDatabase.Properties.Any() ? MockDatabase.Properties.Max(p => p.Id) + 1 : 1;

        var newProperty = new Property
        {
            Id = newId,
            Address = dto.Address,
            Price = dto.Price,
            RoomsCount = dto.RoomsCount,
            IsAvailable = dto.IsAvailable
        };

        MockDatabase.Properties.Add(newProperty);

        // 201 Created з Location header
        return CreatedAtAction(nameof(GetProperty), new { id = newProperty.Id }, newProperty);
    }

    // PUT: api/properties/1
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateProperty(int id, [FromBody] PropertyDto dto)
    {
        var existingProperty = MockDatabase.Properties.FirstOrDefault(p => p.Id == id);

        if (existingProperty == null)
            return NotFound(); // 404 Not Found

        // Оновлюємо властивості знайденого об'єкта
        existingProperty.Address = dto.Address;
        existingProperty.Price = dto.Price;
        existingProperty.RoomsCount = dto.RoomsCount;
        existingProperty.IsAvailable = dto.IsAvailable;

        return NoContent(); // 204 No Content
    }

    // DELETE: api/properties/1
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteProperty(int id)
    {
        var propertyToRemove = MockDatabase.Properties.FirstOrDefault(p => p.Id == id);

        if (propertyToRemove != null)
        {
            MockDatabase.Properties.Remove(propertyToRemove);
            return NoContent(); // 204 No Content
        }

        return NotFound(); // 404 Not Found
    }
}