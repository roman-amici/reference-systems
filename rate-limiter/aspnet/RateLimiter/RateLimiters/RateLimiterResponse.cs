namespace RateLimiters;

public class RateLimiterResponse
{
    public bool Allowed { get; set; }
    public uint TryAgainInSeconds { get; set; }
}