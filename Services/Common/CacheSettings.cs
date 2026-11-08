namespace RealEstateApi.Services.Common;

public class CacheSettings
{
    public bool Enabled { get; set; } = true;
    public long SizeLimit { get; set; } = 1000;
    public int EntityAbsoluteExpirationMinutes { get; set; } = 10;
    public int EntitySlidingExpirationMinutes { get; set; } = 3;
    public int CurrencyRatesExpirationMinutes { get; set; } = 60;
}
