using Microsoft.AspNetCore.Mvc;
using RateLimiters;

namespace aspnet.Controllers;

[ApiController]
[Route("[controller]")]
public class RequestController : ControllerBase
{
    private readonly ILogger<RequestController> _logger;
    private readonly IRateLimiter _rateLimiter;

    public RequestController(ILogger<RequestController> logger, IRateLimiter rateLimiter)
    {
        _logger = logger;
        _rateLimiter = rateLimiter;
    }

    [HttpGet(Name = "GetRequest")]
    [Route("{requestId}")]
    public Task<RateLimiterResponse> Get(string requestId)
    {
        return _rateLimiter.IsAllowed(requestId);
    }
}
