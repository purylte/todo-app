using System.Linq.Expressions;
using System.Net;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Exceptions;
using TodoApi.Models;

namespace TodoApi.Services;

public interface ITodoService
{
    List<TodoResponse> GetAllUserTodo(GetTodoRequest request);
    TodoResponse GetUserTodoById(int id);
    TodoResponse CreateTodo(CreateTodoRequest request);
    TodoResponse UpdateTodo(UpdateTodoRequest request);
}

public class TodoService : ITodoService
{
    private readonly TodoApiContext _context;
    private readonly IUserService _userService;

    public TodoService(TodoApiContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public List<TodoResponse> GetAllUserTodo(GetTodoRequest request)
    {
        var user = _userService.GetCurrentUser();

        var userTodo = _context.Todo.Where(t => t.User.Equals(user));

        if (request.IsDone != null)
            userTodo = userTodo.Where(t => t.IsDone.Equals(request.IsDone));

        if (request.TimeCreatedRange != null)
            userTodo = userTodo.Where(t =>
                t.TimeCreated <= request.TimeCreatedRange.EndDate &&
                t.TimeCreated >= request.TimeCreatedRange.StartDate);

        if (request.LastUpdatedRange != null)
            userTodo = userTodo.Where(t =>
                t.LastUpdated <= request.LastUpdatedRange.EndDate &&
                t.LastUpdated >= request.LastUpdatedRange.StartDate);


        IOrderedQueryable<Todo> orderedUserTodo;
        var sortByExpressions = new Dictionary<SortByOption, Expression<Func<Todo, object>>>
        {
            { SortByOption.TimeCreated, t => t.TimeCreated },
            { SortByOption.LastUpdated, t => t.LastUpdated },
            { SortByOption.Body, t => t.Body },
            { SortByOption.Done, t => t.IsDone }
        };

        if (!sortByExpressions.ContainsKey(request.SortBy)) 
            throw new ApiException { ErrorMessage = "SortByOption not recognized" };
        
        var orderByExpression = sortByExpressions[request.SortBy];

        if (request.SortDirection == SortDirection.Asc)
            orderedUserTodo = userTodo.OrderBy(orderByExpression);
        else
            orderedUserTodo = userTodo.OrderByDescending(orderByExpression);


        var responses = new List<TodoResponse>();
        foreach (var todo in orderedUserTodo.ToList())
        {
            responses.Add(new TodoResponse{
                Id = todo.Id,
                Body = todo.Body,
                IsDone = todo.IsDone,
                LastUpdated = todo.LastUpdated,
                TimeCreated = todo.TimeCreated
            });
        }

        return responses;
    }

    public TodoResponse CreateTodo(CreateTodoRequest request)
    {
        var user = _userService.GetCurrentUser();

        var todo = new Todo {
            User = user,
            Body = request.Body,
            IsDone = request.IsDone
        };
        _context.Todo.Add(todo);
        _context.SaveChanges();
        _context.Todo.Entry(todo).Reload();
        user.Todos ??= new List<Todo>(); 
        user.Todos.Add(todo); 
        _context.SaveChanges();
        return new TodoResponse
        {
            Id = todo.Id,
            Body = todo.Body,
            IsDone = todo.IsDone,
            LastUpdated = todo.LastUpdated,
            TimeCreated = todo.TimeCreated
        };
    }

    public TodoResponse GetUserTodoById(int id)
    {
        var user = _userService.GetCurrentUser();
        var todo = _context.Todo.SingleOrDefault(t => t.User.Equals(user) && t.Id.Equals(id));
        if (todo is null) throw new ApiException {
            ErrorMessage = $"Todo with id {id} is not found.", 
            StatusCode = HttpStatusCode.NotFound
        };
        return new TodoResponse
        {
            Body = todo.Body,
            Id = todo.Id,
            IsDone = todo.IsDone,
            LastUpdated = todo.LastUpdated,
            TimeCreated = todo.TimeCreated
        };
    }

    public TodoResponse UpdateTodo(UpdateTodoRequest request)
    {
        var user = _userService.GetCurrentUser();
        var todo = _context.Todo.SingleOrDefault(t => t.User.Equals(user) && t.Id.Equals(request.Id));
        if (todo is null) throw new ApiException {
            ErrorMessage = $"Todo with id {request.Id} is not found.", 
            StatusCode = HttpStatusCode.NotFound};
        todo.IsDone = request.IsDone;
        todo.Body = request.Body;
        _context.SaveChanges();
        _context.Todo.Entry(todo).Reload();
        return new TodoResponse
        {
            Id = todo.Id,
            Body = todo.Body,
            IsDone = todo.IsDone,
            LastUpdated = todo.LastUpdated,
            TimeCreated = todo.TimeCreated
        };
    }
}


    