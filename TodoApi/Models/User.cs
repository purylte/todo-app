namespace TodoApi.Models;

public class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public virtual ICollection<Todo> Todos { get; } = new List<Todo>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();
}