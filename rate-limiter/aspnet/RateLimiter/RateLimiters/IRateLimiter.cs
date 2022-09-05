namespace RateLimiters;

public interface IRateLimiter
{
    public Task<RateLimiterResponse> IsAllowed(string requestId);
}


