using Microsoft.AspNetCore.Mvc;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Deals;
using RealEstateApi.Models;
using Microsoft.EntityFrameworkCore;

namespace RealEstateApi.Controllers;

[ApiController]
[Route("api/[controller]")]

public class DealsController : ControllerBase
{

    private readonly ApplicationDbContext _context;
    public DealsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/deals
    [HttpGet]
    public async Task<IActionResult> GetAllDeals([FromQuery] string? status)
    {
        var query = _context.Deals
            .Include(d => d.Property)
            .Include(d => d.Client)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(d => d.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        var deals = await query.ToListAsync();
        return Ok(deals);
    }

    // GET: api/deals/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDealById(int id)
    {
        var deal = await _context.Deals
            .Include(d => d.Property)
            .Include(d => d.Client)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deal == null)
            return NotFound(new { message = $"Deal with id {id} not found" });

        return Ok(deal);
    }

    // POST: api/deals
    [HttpPost]
    public async Task<IActionResult> CreateDeal([FromBody] CreateDealDto dto)
    {
        var newDeal = new Deal
        {
            PropertyId = dto.PropertyId,
            ClientId = dto.ClientId,
            RealtorId = dto.RealtorId,
            Price = dto.Price,
            Date = DateTime.UtcNow,
            Status = "Pending"
        };

        _context.Deals.Add(newDeal);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDealById), new { id = newDeal.Id }, newDeal);
    }

    // PUT: api/deals/5'
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDeal(int id, [FromBody] UpdateDealDto dto)
    {
        var deal = await _context.Deals.FindAsync(id);

        if (deal == null)
            return NotFound();

        deal.Price = dto.Price;
        deal.Status = dto.Status;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/deals/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDeal(int id)
    {
        var deal = await _context.Deals.FindAsync(id);

        if (deal == null)
            return NotFound();

        _context.Deals.Remove(deal);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}