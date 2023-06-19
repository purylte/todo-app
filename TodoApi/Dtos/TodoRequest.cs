namespace TodoApi.Dtos;

public class TodoRequest
{
    public required string Body { get; set; }
    
    public required bool IsDone { get; set; }
    
}