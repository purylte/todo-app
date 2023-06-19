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
    public IActionResult CreateTodo(TodoRequest request)
    {
        var todo = _todoService.CreateUserTodo(request);
        return CreatedAtAction(nameof(GetTodoById), new { id = todo.Id }, todo);
    }
    
    [HttpPut("{id}")]
    public IActionResult CreateTodo(int id, TodoRequest request)
    {
        var todo = _todoService.UpdateUserTodoById(id, request);
        return Ok(todo);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteTodo(int id)
    {
        _todoService.DeleteUserTodoById(id);
        return NoContent();
    }
}