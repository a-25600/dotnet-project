namespace RealEstateApi.Services.Common;

/// <summary>
/// Статичні константи системних ролей доступу.
/// </summary>
public static class AppRoles
{
    /// <summary>
    /// Роль адміністратора з повним доступом до керування та видалення даних.
    /// </summary>
    public const string Administrator = "Administrator";

    /// <summary>
    /// Базова роль користувача системи за замовчуванням.
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// Комбінований рядок для дозволу доступу користувачам обох ролей.
    /// </summary>
    public const string UserOrAdministrator = "User,Administrator";
}