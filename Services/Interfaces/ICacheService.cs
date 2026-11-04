namespace RealEstateApi.Services.Interfaces;

public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(
        string region,
        string key,
        Func<CancellationToken, Task<T>> factory,
        Func<T, bool>? shouldCache = null,
        CancellationToken cancellationToken = default);

    void InvalidateRegions(params string[] regions);
}
