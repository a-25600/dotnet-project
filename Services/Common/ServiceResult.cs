namespace RealEstateApi.Services.Common;

/// <summary>
/// Статуси виконання операцій сервісного шару.
/// </summary>
public enum ServiceStatus
{
    /// <summary>
    /// Операція завершилась успішно (200 OK або 204 NoContent).
    /// </summary>
    Success,

    /// <summary>
    /// Ресурс успішно створено (201 Created).
    /// </summary>
    Created,

    /// <summary>
    /// Ресурс не знайдено (404 Not Found).
    /// </summary>
    NotFound,

    /// <summary>
    /// Помилка валідації або бізнес-правил (400 Bad Request).
    /// </summary>
    BadRequest,

    /// <summary>
    /// Конфлікт даних, порушення унікальності або зв'язків (409 Conflict).
    /// </summary>
    Conflict
}

/// <summary>
/// Результат виконання операції сервісного шару без повернення даних (для Update, Delete).
/// </summary>
public class ServiceResult
{
    public ServiceStatus Status { get; init; }
    public string? ErrorMessage { get; init; }

    public bool IsSuccess => Status is ServiceStatus.Success or ServiceStatus.Created;

    public static ServiceResult Ok() => new() { Status = ServiceStatus.Success };
    public static ServiceResult NotFound(string message = "Запис не знайдено.") => new() { Status = ServiceStatus.NotFound, ErrorMessage = message };
    public static ServiceResult BadRequest(string message) => new() { Status = ServiceStatus.BadRequest, ErrorMessage = message };
    public static ServiceResult Conflict(string message) => new() { Status = ServiceStatus.Conflict, ErrorMessage = message };
}

/// <summary>
/// Результат виконання операції сервісного шару з даними.
/// </summary>
/// <typeparam name="T">Тип даних результату.</typeparam>
public class ServiceResult<T>
{
    public ServiceStatus Status { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }

    public bool IsSuccess => Status is ServiceStatus.Success or ServiceStatus.Created;

    public static ServiceResult<T> Ok(T data) => new() { Status = ServiceStatus.Success, Data = data };
    public static ServiceResult<T> Created(T data) => new() { Status = ServiceStatus.Created, Data = data };
    public static ServiceResult<T> NotFound(string message = "Запис не знайдено.") => new() { Status = ServiceStatus.NotFound, ErrorMessage = message };
    public static ServiceResult<T> BadRequest(string message) => new() { Status = ServiceStatus.BadRequest, ErrorMessage = message };
    public static ServiceResult<T> Conflict(string message) => new() { Status = ServiceStatus.Conflict, ErrorMessage = message };
}
