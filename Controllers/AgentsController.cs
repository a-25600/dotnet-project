using Microsoft.AspNetCore.Mvc;
using RealEstateApi.Data;
using RealEstateApi.DTOs.Agents;
using RealEstateApi.Models;

namespace RealEstateApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgentsController : ControllerBase
{
    private static AgentDto ToDto(Agent agent) => new()
    {
        Id = agent.Id,
        FullName = agent.FullName,
        Email = agent.Email,
        PhoneNumber = agent.PhoneNumber,
        LicenseNumber = agent.LicenseNumber,
        IsActive = agent.IsActive
    };

    // GET: api/agents?isActive=true
    [HttpGet]
    public IActionResult GetAgents([FromQuery] AgentQueryParameters parameters)
    {
        var agents = MockDatabase.Agents.AsQueryable();

        if (parameters.IsActive.HasValue)
            agents = agents.Where(a => a.IsActive == parameters.IsActive.Value);

        return Ok(agents.Select(ToDto).ToList());
    }

    [HttpGet("{id}")]
    public IActionResult GetAgentById(int id)
    {
        var agent = MockDatabase.Agents.FirstOrDefault(a => a.Id == id);

        if (agent == null)
            return NotFound(new { message = $"Agent with id {id} not found" });

        return Ok(ToDto(agent));
    }

    [HttpPost]
    public IActionResult CreateAgent([FromBody] CreateAgentDto dto)
    {
        var newAgent = new Agent
        {
            Id = MockDatabase.Agents.Any() ? MockDatabase.Agents.Max(a => a.Id) + 1 : 1,
            FullName = dto.FullName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            LicenseNumber = dto.LicenseNumber,
            IsActive = true
        };

        MockDatabase.Agents.Add(newAgent);

        return CreatedAtAction(nameof(GetAgentById), new { id = newAgent.Id }, ToDto(newAgent));
    }

    [HttpPut("{id}")]
    public IActionResult UpdateAgent(int id, [FromBody] UpdateAgentDto dto)
    {
        var agent = MockDatabase.Agents.FirstOrDefault(a => a.Id == id);

        if (agent == null)
            return NotFound();

        agent.FullName = dto.FullName;
        agent.Email = dto.Email;
        agent.PhoneNumber = dto.PhoneNumber;
        agent.LicenseNumber = dto.LicenseNumber;
        agent.IsActive = dto.IsActive;

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteAgent(int id)
    {
        var agent = MockDatabase.Agents.FirstOrDefault(a => a.Id == id);

        if (agent == null)
            return NotFound();

        MockDatabase.Agents.Remove(agent);

        return NoContent();
    }
}