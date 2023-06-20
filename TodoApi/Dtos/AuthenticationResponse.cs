using System.Text.Json.Serialization;

namespace TodoApi.Dtos;

public class AuthenticationResponse
{
    public required string Username { get; set; }
    public required string JwtToken { get; set; }
    
    [JsonIgnore]
    public string RefreshToken { get; init; }
    
}