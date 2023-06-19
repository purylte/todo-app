namespace TodoApi.Dtos;

public class TodoResponse
{
    public required int Id { get; set; }
    public required string Body { get; set; }
    
    public required bool IsDone { get; set; }
    
    public required DateTime TimeCreated { get; set; }
    
    public required DateTime? LastUpdated { get; set; }
}