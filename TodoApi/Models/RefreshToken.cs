namespace TodoApi.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public DateTime TimeCreated { get; } = DateTime.Now.ToUniversalTime();
    public required string Token { get; init; }
    public required DateTime Expires { get; init; }
    public bool IsRevoked { get; set; }
    
    public int UserId { get; set; }
}