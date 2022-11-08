namespace Mvc.SourceGen.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using Mvc.SourceGen.Web.Models;

[Route("[controller]")]
[ApiController]
public class TodosController : ControllerBase
{
    private static readonly List<Todo> Todos = new();

    [HttpGet]
    public IEnumerable<Todo> GetAll() 
        => Todos;

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        var  lookUpId = $"#{id}";

        var todo = Todos.SingleOrDefault(x => x.Id == lookUpId);
        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpPost]
    public IActionResult Add(Todo todo)
    {
        todo.Id = $"#{Todos.Count + 1}";

        Todos.Add(todo);

        //  System.InvalidOperationException: No route matches the supplied values.
        //  at Microsoft.AspNetCore.Mvc.CreatedAtActionResult.OnFormatting(ActionContext)
        //return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);

        return Created("todos", todo);
    }
}
