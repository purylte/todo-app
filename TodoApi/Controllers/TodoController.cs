using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Dtos;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Authorize(Roles = "User")]
[Route("api/todo")]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }
    
    [HttpGet("")]
    public IActionResult GetTodos([FromQuery] GetTodoRequest request)
    {
        var todos = _todoService.GetAllUserTodo(request);
        return Ok(todos);
    }
    
    [HttpGet("{id}")]
    public IActionResult GetTodoById(int id)
    {
        var todo = _todoService.GetUserTodoById(id);
        return Ok(todo);
    }

    [HttpPost("")]
    public IActionResult CreateTodo(CreateTodoRequest request)
    {
        var todo = _todoService.CreateTodo(request);
        return CreatedAtAction(nameof(GetTodoById), new { id = todo.Id }, todo);
    }
    
    [HttpPut("")]
    public IActionResult CreateTodo(UpdateTodoRequest request)
    {
        var todo = _todoService.UpdateTodo(request);
        return Ok(todo);
    }
}