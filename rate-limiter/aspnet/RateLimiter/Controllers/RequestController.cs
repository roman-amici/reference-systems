using Microsoft.AspNetCore.Mvc;

namespace aspnet.Controllers;

[ApiController]
[Route("[controller]")]
public class RequestController : ControllerBase
{
    private readonly ILogger<RequestController> _logger;

    public RequestController(ILogger<RequestController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetRequest")]
    public async Task<bool> Get()
    {
        return false;
    }
}
