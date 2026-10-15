namespace RealEstateApi.DTOs.Agents;

/// <summary>
/// Представлення ріелтора для списків та відповідей API.
/// </summary>
public class AgentDto
{
    /// <summary>
    /// Ідентифікатор ріелтора.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ПІБ ріелтора.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Контактний email ріелтора.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Робочий номер телефону.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Номер ліцензії ріелтора.
    /// </summary>
    public string LicenseNumber { get; set; } = string.Empty;

    /// <summary>
    /// Чи активний статус співробітника.
    /// </summary>
    public bool IsActive { get; set; }
}