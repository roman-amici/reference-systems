using System.Collections.Concurrent;

namespace RateLimiters.InMemoryRateLimiters;

public class TokenBucket : IRateLimiter
{
    public class CacheEntry
    {
        public uint Tokens { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }

    public uint ReplenishmentPeriodSeconds { get; private set; } = 5;
    public uint MaxTokens { get; private set; } = 10;
    public uint CacheDurationSeconds { get; set; } = 60;

    ConcurrentDictionary<string, CacheEntry> Cache { get; set; } =
        new ConcurrentDictionary<string, CacheEntry>();

    Timer RefreshTimer { get; set; }

    public TokenBucket(
        uint replenishmentPeriodSeconds,
        uint maxTokens,
        uint cacheDurationSeconds)
    {
        ReplenishmentPeriodSeconds = replenishmentPeriodSeconds;
        MaxTokens = maxTokens;
        CacheDurationSeconds = cacheDurationSeconds;

        // Don't start the timer until "StartRefreshTimer" is called
        RefreshTimer = new Timer(AddTokens, null, Timeout.Infinite, Timeout.Infinite);

    }

    public void StartRefreshTimer()
    {
        RefreshTimer.Change(0, ReplenishmentPeriodSeconds * 1000);
    }

    public void StopRefreshTimer()
    {
        RefreshTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public void AddTokens(DateTime now)
    {
        var keysToRemove = new List<string>();
        foreach (var kvp in Cache)
        {
            var entry = kvp.Value;
            lock (entry)
            {
                var duration = now - entry.LastUpdateTime;
                if (duration > new TimeSpan(0, 0, (int)CacheDurationSeconds))
                {
                    keysToRemove.Add(kvp.Key);
                }
                else
                {
                    if (entry.Tokens < MaxTokens)
                    {
                        entry.Tokens += 1;
                    }
                }
            }
        }

        foreach (var key in keysToRemove)
        {
            Cache.TryRemove(key, out CacheEntry _);
        }
    }

    public bool EntryIsPresent(string entryId)
    {
        return Cache.ContainsKey(entryId);
    }

    public void AddTokens(object? _state)
    {
        AddTokens(DateTime.UtcNow);
    }

    public Task<RateLimiterResponse> IsAllowed(string requestId)
    {
        return IsAllowed(requestId, DateTime.UtcNow);
    }

    public Task<RateLimiterResponse> IsAllowed(string requestId, DateTime now)
    {
        var cacheEntry = Cache.GetOrAdd(
            requestId,
            new CacheEntry()
            {
                Tokens = MaxTokens,
                LastUpdateTime = now
            });

        lock (cacheEntry)
        {
            if (cacheEntry.Tokens == 0)
            {
                return Task.FromResult(new RateLimiterResponse()
                {
                    Allowed = false,
                    TryAgainInSeconds = ReplenishmentPeriodSeconds
                });
            }

            cacheEntry.Tokens--;
            cacheEntry.LastUpdateTime = now;
            return Task.FromResult(new RateLimiterResponse()
            {
                Allowed = true,
                TryAgainInSeconds = 0
            });
        }
    }
}
