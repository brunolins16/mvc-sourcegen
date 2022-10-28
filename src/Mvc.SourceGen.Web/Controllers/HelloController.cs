using Microsoft.AspNetCore.Mvc;

namespace Mvc.SourceGen.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet(Name = "SayHello")]
    public string Get()
    {
        return "Hello World";
    }
}
