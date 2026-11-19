using System.Linq.Expressions;
using UsersService.DTOs.Agents;
using UsersService.DTOs.Clients;
using UsersService.Models;

namespace UsersService.Mappings;

public static class MappingExtensions
{
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static ClientDto ToDto(this Client client) => new()
    {
        Id = client.Id,
        FullName = client.FullName,
        Email = client.Email,
        PhoneNumber = client.PhoneNumber
    };

    public static ClientDetailsDto ToDetailsDto(this Client client) => new()
    {
        Id = client.Id,
        FullName = client.FullName,
        Email = client.Email,
        PhoneNumber = client.PhoneNumber
    };

    public static Client ToEntity(this CreateClientDto dto) => new()
    {
        FullName = dto.FullName.Trim(),
        Email = NormalizeEmail(dto.Email),
        PhoneNumber = dto.PhoneNumber.Trim()
    };

    public static void UpdateFrom(this Client client, UpdateClientDto dto)
    {
        client.FullName = dto.FullName.Trim();
        client.Email = NormalizeEmail(dto.Email);
        client.PhoneNumber = dto.PhoneNumber.Trim();
    }

    public static AgentDto ToDto(this Agent agent) => new()
    {
        Id = agent.Id,
        FullName = agent.FullName,
        Email = agent.Email,
        PhoneNumber = agent.PhoneNumber,
        LicenseNumber = agent.LicenseNumber,
        IsActive = agent.IsActive
    };

    public static AgentDetailsDto ToDetailsDto(this Agent agent) => new()
    {
        Id = agent.Id,
        FullName = agent.FullName,
        Email = agent.Email,
        PhoneNumber = agent.PhoneNumber,
        LicenseNumber = agent.LicenseNumber,
        IsActive = agent.IsActive
    };

    public static Agent ToEntity(this CreateAgentDto dto) => new()
    {
        FullName = dto.FullName.Trim(),
        Email = NormalizeEmail(dto.Email),
        PhoneNumber = dto.PhoneNumber.Trim(),
        LicenseNumber = dto.LicenseNumber.Trim(),
        IsActive = true
    };

    public static void UpdateFrom(this Agent agent, UpdateAgentDto dto)
    {
        agent.FullName = dto.FullName.Trim();
        agent.Email = NormalizeEmail(dto.Email);
        agent.PhoneNumber = dto.PhoneNumber.Trim();
        agent.LicenseNumber = dto.LicenseNumber.Trim();
        agent.IsActive = dto.IsActive;
    }
}