using RealEstateApi.DTOs.Properties;
using RealEstateApi.Services.Common;

namespace RealEstateApi.Services.Interfaces;

/// <summary>
/// Сервіс для керування об'єктами нерухомості.
/// </summary>
public interface IPropertyService
{
    /// <summary>
    /// Отримати список об'єктів нерухомості з фільтрацією та сортуванням.
    /// </summary>
    Task<IReadOnlyList<PropertyDto>> GetAllAsync(PropertyQueryParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримати детальну інформацію про об'єкт нерухомості за ідентифікатором разом з його угодами.
    /// </summary>
    Task<ServiceResult<PropertyDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Створити новий об'єкт нерухомості.
    /// </summary>
    Task<ServiceResult<PropertyDto>> CreateAsync(CreatePropertyDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Оновити дані існуючого об'єкта нерухомості.
    /// </summary>
    Task<ServiceResult> UpdateAsync(int id, UpdatePropertyDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Видалити об'єкт нерухомості за ідентифікатором.
    /// </summary>
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
