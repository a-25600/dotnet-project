using RealEstateApi.DTOs.Deals;
using RealEstateApi.Services.Common;

namespace RealEstateApi.Services.Interfaces;

/// <summary>
/// Сервіс для керування угодами агентства нерухомості.
/// </summary>
public interface IDealService
{
    /// <summary>
    /// Отримати список угод з фільтрацією та сортуванням.
    /// </summary>
    Task<IReadOnlyList<DealDto>> GetAllAsync(DealQueryParameters parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримати дані угоди за ідентифікатором.
    /// </summary>
    Task<ServiceResult<DealDto>> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Створити нову угоду з перевіркою доступності об'єкта, клієнта та ріелтора.
    /// </summary>
    Task<ServiceResult<DealDto>> CreateAsync(CreateDealDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Оновити параметри угоди (ціну або статус).
    /// </summary>
    Task<ServiceResult> UpdateAsync(int id, UpdateDealDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Видалити угоду за ідентифікатором.
    /// </summary>
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
