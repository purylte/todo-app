namespace TodoApi.Models;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public List<Todo>? Todos { get; set; }
}