# Source code

```csharp
namespace TodoApp.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class TodosController : ControllerBase
{
    private static readonly System.Collections.Concurrent.ConcurrentBag<Todo> Todos = new();

    [HttpGet()]
    public async Task<Todo[]> GetAll([FromQuery] TodoFilterRequest filter)
    {
        // code omitted
    }

    [HttpGet("{id}")]
    public ActionResult<Todo> Get(int id)
    {
        // code omitted
    }

    [HttpPost]
    public IActionResult Post(NewTodo todo)
    {
        // code omitted
    }

    [HttpPost("batch")]
    public IActionResult Post([FromBody]List<NewTodo> todos)
    {
        // code omitted
    }
}

public record Todo(int Id, string Title, string? Description, DateTime? StartDate, bool Completed);

public record struct NewTodo(string Title, string? Description);

public record TodoFilterRequest(string? Title, bool? Completed);
```
