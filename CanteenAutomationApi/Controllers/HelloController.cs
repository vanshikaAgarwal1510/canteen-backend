using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/hello")]
public class HelloController : ControllerBase
{
    [HttpPost]
    public IActionResult SayHello([FromBody] HelloRequest request)
    {
        return Ok($"Hello {request.Name} 👋");
    }
}

public class HelloRequest
{


    public required string Name { get; set; }
}
