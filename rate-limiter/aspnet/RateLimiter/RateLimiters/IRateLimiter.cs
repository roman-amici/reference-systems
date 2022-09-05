namespace RateLimiters;

public interface IRateLimiter
{
    Task<RateLimiterResponse> IsAllowed(string requestId);
}


