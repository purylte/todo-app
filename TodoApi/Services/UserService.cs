using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Exceptions;
using TodoApi.Models;

namespace TodoApi.Services;

public interface IUserService
{
    void Register(AuthenticationRequest request);
    AuthenticationResponse Authenticate(AuthenticationRequest request);
    User GetCurrentUser();
}

public class UserService : IUserService
{
    private readonly IConfiguration _configuration;
    
    private readonly TodoApiContext _context;

    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public UserService(TodoApiContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public void Register(AuthenticationRequest request)
    {
        if (_context.User.AsEnumerable().SingleOrDefault(u => u.Username.Equals(request.Username)) != null)
        {
            throw new ApiException{
                ErrorMessage = "Username already exist",
                StatusCode = HttpStatusCode.Conflict};
        }

        _context.User.Add(new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        });
        
        _context.SaveChangesAsync();
    }

    public AuthenticationResponse Authenticate(AuthenticationRequest request)
    {
        var user = _context.User.AsEnumerable().SingleOrDefault(u => u.Username.Equals(request.Username));
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new ApiException{
                ErrorMessage = "Username or password is incorrect",
                StatusCode = HttpStatusCode.Unauthorized,
            };

        var jwtToken = GenerateUserToken(user);

        return new AuthenticationResponse
        {
            Username = user.Username,
            JwtToken = jwtToken,
        };
    }

    public User GetCurrentUser()
    {
        if (_httpContextAccessor.HttpContext is null) throw new ApiException{ ErrorMessage = "HttpContext is null"};
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
        if (userId is null) throw new ApiException {ErrorMessage = "Current user not found"};
        var parsedUserId = int.Parse(userId);
        return _context.User.First(u => u.Id.Equals(parsedUserId));
    }
    private string GenerateUserToken(User user)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Username),
            new Claim(ClaimTypes.Role, "User")
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetSection("AppSettings:Secret").Value!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: credentials
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }
}