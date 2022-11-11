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

    [HttpPost]
    public IActionResult Add(Todo todo)
    {
        Todos.Add(todo);

        //  System.InvalidOperationException: No route matches the supplied values.
        //  at Microsoft.AspNetCore.Mvc.CreatedAtActionResult.OnFormatting(ActionContext)
        //return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);

        return Created("todos", todo);
    }
}
