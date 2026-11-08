using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using RealEstateApi.Services.Common;
using RealEstateApi.Services.Interfaces;

namespace RealEstateApi.Services.Implementations;

public sealed class MemoryCacheService : ICacheService, IDisposable
{
    private readonly MemoryCache _cache;
    private readonly CacheSettings _settings;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _regionTokens = new();

    public MemoryCacheService(IOptions<CacheSettings> settings, ILogger<MemoryCacheService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = _settings.SizeLimit });
    }

    public async Task<T> GetOrCreateAsync<T>(
        string region,
        string key,
        Func<CancellationToken, Task<T>> factory,
        Func<T, bool>? shouldCache = null,
        CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
            return await factory(cancellationToken);

        var fullKey = $"{region}:{key}";

        if (TryGet(fullKey, out T? cached))
        {
            _logger.LogInformation("Cache hit: {CacheKey}", fullKey);
            return cached!;
        }

        _logger.LogInformation("Cache miss: {CacheKey}", fullKey);

        var regionToken = GetRegionToken(region);
        var value = await factory(cancellationToken);

        if (shouldCache == null || shouldCache(value))
            TrySet(fullKey, value, region, regionToken);

        return value;
    }

    public void InvalidateRegions(params string[] regions)
    {
        foreach (var region in regions.Distinct())
        {
            if (!_regionTokens.TryRemove(region, out var source))
                continue;

            try
            {
                source.Cancel();
                _logger.LogInformation("Cache region invalidated: {CacheRegion}", region);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate cache region {CacheRegion}", region);
            }
        }
    }

    public void Dispose() => _cache.Dispose();

    private CancellationToken GetRegionToken(string region) =>
        _regionTokens.GetOrAdd(region, _ => new CancellationTokenSource()).Token;

    private bool TryGet<T>(string fullKey, out T? value)
    {
        try
        {
            if (_cache.TryGetValue(fullKey, out var stored) && stored is T typed)
            {
                value = typed;
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for {CacheKey}, falling back to data source", fullKey);
        }

        value = default;
        return false;
    }

    private void TrySet<T>(string fullKey, T value, string region, CancellationToken regionToken)
    {
        try
        {
            var options = new MemoryCacheEntryOptions { Size = 1 };

            if (region == CacheRegions.Currency)
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.CurrencyRatesExpirationMinutes);
            }
            else
            {
                options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.EntityAbsoluteExpirationMinutes);
                options.SlidingExpiration = TimeSpan.FromMinutes(_settings.EntitySlidingExpirationMinutes);
            }

            options.AddExpirationToken(new CancellationChangeToken(regionToken));

            _cache.Set(fullKey, value, options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache write failed for {CacheKey}", fullKey);
        }
    }
}
