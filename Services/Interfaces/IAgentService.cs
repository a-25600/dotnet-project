using RealEstateApi.DTOs.Agents;
using RealEstateApi.Services.Common;

namespace RealEstateApi.Services.Interfaces;

/// <summary>
/// Сервіс для керування ріелторами агентства нерухомості.
/// </summary>
public interface IAgentService
{
    /// <summary>
    /// Отримати список ріелторів з фільтром за активністю, пошуком і сортуванням.
    /// </summary>
    Task<IReadOnlyList<AgentDto>> GetAllAsync(AgentQueryParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримати ріелтора за ідентифікатором разом зі списком його угод.
    /// </summary>
    Task<ServiceResult<AgentDetailsDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Зареєструвати нового ріелтора з перевіркою унікальності email та ліцензії.
    /// </summary>
    Task<ServiceResult<AgentDto>> CreateAsync(CreateAgentDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Оновити дані ріелтора.
    /// </summary>
    Task<ServiceResult> UpdateAsync(int id, UpdateAgentDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Видалити ріелтора (за відсутності пов'язаних угод).
    /// </summary>
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
