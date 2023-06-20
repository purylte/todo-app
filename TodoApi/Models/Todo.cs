namespace TodoApi.Models;

public class Todo
{
    public int Id { get; set; }
    
    public required string Body { get; set; }
    
    public required bool IsDone { get; set; }
    
    public DateTime TimeCreated { get; set; } = DateTime.Now.ToUniversalTime();
    
    public DateTime LastUpdated { get; set; } = DateTime.Now.ToUniversalTime();

    public int UserId { get; set; }
}