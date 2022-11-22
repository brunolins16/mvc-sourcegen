namespace Mvc.SourceGen.Web.Controllers;

using Microsoft.AspNetCore.Mvc;
using Mvc.SourceGen.Web.Models;

[Route("[controller]")]
[ApiController]
public class TodosController : ControllerBase
{
    private static readonly List<Todo> Todos = new();

    //[HttpGet]
    //public IEnumerable<Todo> GetAll(ParsableTodo todo)
    //    => Todos;

    //[HttpGet("{id}")]
    //public IEnumerable<Todo> GetById(int id, TodoType type)
    //=> Todos;

    //[HttpGet("delayed")]
    //public async Task<IEnumerable<Todo>> GetAllAsync()
    //{
    //    await Task.Delay(1000);
    //    return Todos;
    //}

    [HttpPost]
    public IActionResult Add(bool? todo) => Ok();
    //{
    //    Todos.Add(todo);

    //    //  System.InvalidOperationException: No route matches the supplied values.
    //    //  at Microsoft.AspNetCore.Mvc.CreatedAtActionResult.OnFormatting(ActionContext)
    //    //return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);

    //    return Created("todos", todo);
    //}

    public IActionResult Add(List<Todo> todo) => Ok();
}

