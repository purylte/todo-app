namespace TodoApi.Dtos;

public class UpdateTodoRequest
{
    public required int Id { get; set; }
    
    public required string Body { get; set; }
    
    public required bool IsDone { get; set; }
    
}