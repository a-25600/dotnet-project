using System.Linq.Expressions;
using CatalogService.DTOs.Properties;
using CatalogService.Models;

namespace CatalogService.Mappings;

public static class MappingExtensions
{
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static PropertyDto ToDto(this Property property) => new()
    {
        Id = property.Id,
        Address = property.Address,
        Price = property.Price,
        Type = property.Type,
        RoomsCount = property.RoomsCount,
        IsAvailable = property.IsAvailable
    };

    public static PropertyDetailsDto ToDetailsDto(this Property property) => new()
    {
        Id = property.Id,
        Address = property.Address,
        Price = property.Price,
        Type = property.Type,
        RoomsCount = property.RoomsCount,
        IsAvailable = property.IsAvailable
    };

    public static Property ToEntity(this CreatePropertyDto dto) => new()
    {
        Address = dto.Address.Trim(),
        Price = dto.Price,
        Type = dto.Type.Trim(),
        RoomsCount = dto.RoomsCount,
        IsAvailable = dto.IsAvailable
    };

    public static void UpdateFrom(this Property property, UpdatePropertyDto dto)
    {
        property.Address = dto.Address.Trim();
        property.Price = dto.Price;
        property.Type = dto.Type.Trim();
        property.RoomsCount = dto.RoomsCount;
        property.IsAvailable = dto.IsAvailable;
    }
}