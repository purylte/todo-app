namespace TodoApi.Dtos;

public class CreateTodoRequest
{
    public required string Body { get; set; }
    
    public required bool IsDone { get; set; }
    
}