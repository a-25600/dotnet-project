using Microsoft.AspNetCore.Mvc;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Deals;
using RealEstateApi.Models;

namespace RealEstateApi.Controllers;

[ApiController]
[Route("api/[controller]")]

public class DealsController : ControllerBase
{
    // GET: api/deals
    [HttpGet]
    public IActionResult GetAllDeals([FromQuery] string? status)
    {
        var deals = MockDatabase.Deals.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            deals = deals.Where(deals => deals.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(deals.ToList());
    }

    [HttpGet("{id}")]
    public IActionResult GetDealById(int id) 
    {
        var deal = MockDatabase.Deals.FirstOrDefault(deal => deal.Id == id);

        if (deal == null)
            return NotFound(new { message = $"Deal with kd {id} not found" });

        return Ok(deal);
    }

    [HttpPost]
    public IActionResult CreateDeal([FromBody] CreateDealDto dto)
    {
        var newDeal = new Deal
        {
            Id = MockDatabase.Deals.Any() ? MockDatabase.Deals.Max(d => d.Id) + 1 : 1,
            PropertyId = dto.PropertyId,
            ClientId = dto.ClientId,
            RealtorId = dto.RealtorId,
            Price = dto.Price,
            Date = DateTime.UtcNow,
            Status = "Pending"
        };

        MockDatabase.Deals.Add(newDeal);

        return CreatedAtAction(nameof(GetDealById), new { id = newDeal.Id }, newDeal);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateDeal(int id, [FromBody] UpdateDealDto dto)
    {
        var deal = MockDatabase.Deals.FirstOrDefault(d => d.Id == id);

        if (deal == null)
            return NotFound();

        deal.Price = dto.Price;
        deal.Status = dto.Status;

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteDeal(int id)
    {
        var deal = MockDatabase.Deals.FirstOrDefault(d => d.Id == id);

        if (deal == null)
            return NotFound();

        MockDatabase.Deals.Remove(deal);

        return NoContent();
    }
}