using TodoApi.Models;

namespace TodoApi.Dtos;

public class AuthenticationResponse
{
    public required string Username { get; set; }
    public required string JwtToken { get; set; }

    
}