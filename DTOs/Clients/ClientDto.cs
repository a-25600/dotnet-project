namespace RealEstateApi.DTOs.Clients;

/// <summary>
/// Представлення клієнта для списків та відповідей API.
/// </summary>
public class ClientDto
{
    /// <summary>
    /// Ідентифікатор клієнта.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ПІБ клієнта.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Контактний email клієнта.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Номер телефону клієнта.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;
}