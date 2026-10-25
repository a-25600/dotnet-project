using RealEstateApi.DTOs.Clients;
using RealEstateApi.Services.Common;

namespace RealEstateApi.Services.Interfaces;

/// <summary>
/// Сервіс для керування клієнтами агентства.
/// </summary>
public interface IClientService
{
    /// <summary>
    /// Отримати список клієнтів з пошуком і сортуванням.
    /// </summary>
    Task<IReadOnlyList<ClientDto>> GetAllAsync(ClientQueryParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримати клієнта за ідентифікатором разом зі списком його угод.
    /// </summary>
    Task<ServiceResult<ClientDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Зареєструвати нового клієнта з перевіркою унікальності email.
    /// </summary>
    Task<ServiceResult<ClientDto>> CreateAsync(CreateClientDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Оновити контактні дані клієнта.
    /// </summary>
    Task<ServiceResult> UpdateAsync(int id, UpdateClientDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Видалити клієнта (за відсутності пов'язаних угод).
    /// </summary>
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
