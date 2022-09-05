using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RateLimiters.InMemoryRateLimiters;

namespace RateLimiterTest;

[TestClass]
public class InMemoryTokenBucket
{
    [TestMethod]
    public async Task RequestIsAllowed()
    {
        var limiter = new TokenBucket(5, 1, 60);

        var response = await limiter.IsAllowed("12345");

        Assert.IsTrue(response.Allowed);
    }

    [TestMethod]
    public async Task RequestIsBlockedIfThereAreNoTokens()
    {
        var limiter = new TokenBucket(5, 1, 60);

        await limiter.IsAllowed("12345");

        var response = await limiter.IsAllowed("12345");
        Assert.IsFalse(response.Allowed);
    }

    [TestMethod]
    public async Task TokensAreUniqueToUsers()
    {
        var limiter = new TokenBucket(5, 1, 60);

        var response1 = await limiter.IsAllowed("1");
        Assert.IsTrue(response1.Allowed);

        var response2 = await limiter.IsAllowed("2");

        Assert.IsTrue(response2.Allowed);
    }

    [TestMethod]
    public async Task ExpiredEntriesAreRemoved()
    {
        var start = DateTime.Now;
        var maxTime = 60u;
        var limiter = new TokenBucket(5, 1, maxTime);

        await limiter.IsAllowed("1", start);

        var end = start.AddSeconds(maxTime + 1);
        limiter.AddTokens(end);

        Assert.IsFalse(limiter.EntryIsPresent("1"));
    }

    [TestMethod]
    public async Task UnexpiredEntriesAreKept()
    {
        var start = DateTime.Now;
        var maxTime = 60u;
        var limiter = new TokenBucket(5, 1, maxTime);

        await limiter.IsAllowed("1", start);

        var end = start.AddSeconds(maxTime - 1);
        limiter.AddTokens(end);

        Assert.IsTrue(limiter.EntryIsPresent("1"));
    }

    [TestMethod]
    public async Task AddTokensAddsTokens()
    {
        var limiter = new TokenBucket(5, 1, 60);

        await limiter.IsAllowed("1");

        limiter.AddTokens(null);

        var response = await limiter.IsAllowed("1");
        Assert.IsTrue(response.Allowed);
    }

    [TestMethod]
    public async Task AddedTokensDoesNotExceedMaximum()
    {
        var limiter = new TokenBucket(5, 1, 60);

        limiter.AddTokens(null);

        var response1 = await limiter.IsAllowed("1");
        var response2 = await limiter.IsAllowed("1");

        Assert.IsTrue(response1.Allowed);
        Assert.IsFalse(response2.Allowed);
    }
}