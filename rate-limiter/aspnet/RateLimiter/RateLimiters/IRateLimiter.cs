namespace RateLimiters
{
    public interface IRateLimiter
    {
        Task<bool> IsAllowed(string requestId);
    }
}

