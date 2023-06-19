namespace TodoApi.Models;

public class Todo
{
    public int Id { get; set; }
    
    public required User User { get; set; }
    
    public required string Body { get; set; }
    
    public required bool IsDone { get; set; }
    
    public DateTime TimeCreated { get; set; }
    
    public DateTime LastUpdated { get; set; }


}