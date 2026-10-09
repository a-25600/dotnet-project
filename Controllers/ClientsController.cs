using Microsoft.AspNetCore.Mvc;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Clients;
using RealEstateApi.Models;

namespace RealEstateApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private static ClientDto ToDto(Client client) => new()
    {
        Id = client.Id,
        FullName = client.FullName,
        Email = client.Email,
        PhoneNumber = client.PhoneNumber
    };

    [HttpGet]
    public IActionResult GetAllClients()
    {
        return Ok(MockDatabase.Clients.Select(ToDto).ToList());
    }

    [HttpGet("{id}")]
    public IActionResult GetClientById(int id)
    {
        var client = MockDatabase.Clients.FirstOrDefault(c => c.Id == id);

        if (client == null)
            return NotFound(new { message = $"Client with id {id} not found" });

        return Ok(ToDto(client));
    }

    [HttpPost]
    public IActionResult CreateClient([FromBody] CreateClientDto dto)
    {
        var newClient = new Client
        {
            Id = MockDatabase.Clients.Any() ? MockDatabase.Clients.Max(c => c.Id) + 1 : 1,
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        MockDatabase.Clients.Add(newClient);

        return CreatedAtAction(nameof(GetClientById), new { id = newClient.Id }, ToDto(newClient));
    }

    [HttpPut("{id}")]
    public IActionResult UpdateClient(int id, [FromBody] UpdateClientDto dto)
    {
        var client = MockDatabase.Clients.FirstOrDefault(c => c.Id == id);

        if (client == null)
            return NotFound();

        client.FullName = dto.FullName;
        client.Email = dto.Email;
        client.PhoneNumber = dto.PhoneNumber;

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteClient(int id)
    {
        var client = MockDatabase.Clients.FirstOrDefault(c => c.Id == id);

        if (client == null)
            return NotFound();

        MockDatabase.Clients.Remove(client);

        return NoContent();
    }
}