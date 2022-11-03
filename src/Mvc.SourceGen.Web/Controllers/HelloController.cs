using Microsoft.AspNetCore.Mvc;

namespace Mvc.SourceGen.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet(Name = "SayHello")]
    public string SayHello([FromQuery] Message message)
    {
        return message.ToString();
    }

    // TODO: No working yet
    [HttpGet("json", Name = "SayHelloJson")]
    public IActionResult SayHelloJson([FromQuery] Message message)
    {
        return Ok(new MessageResponse() { Text = message.ToString() });
    }
}

public class Message
{
    public string Text { get; set; }
    public string? Name { get; set; }

    public override string ToString()
    {
        return $"{Name ?? "unknown"} says: {Text}";
    }
}

public class MessageResponse
{
    public required string Text { get; init; }
}