using System.Net;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Dtos;
using TodoApi.Exceptions;
using TodoApi.Helpers;
using TodoApi.Models;

namespace TodoApi.Services;

public interface IUserService
{
    void Register(AuthenticationRequest request);
    AuthenticationResponse Authenticate(AuthenticationRequest request);
    AuthenticationResponse RefreshToken(string token);
    void RevokeToken(string token);
    User GetCurrentUser();
}

public class UserService : IUserService
{
    private readonly IJwtUtils _jwtUtils;
    
    private readonly TodoApiContext _context;

    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public UserService(TodoApiContext context, IJwtUtils jwtUtils, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _httpContextAccessor = httpContextAccessor;
    }

    public void Register(AuthenticationRequest request)
    {
        if (_context.Users.AsEnumerable().SingleOrDefault(u => u.Username.Equals(request.Username)) != null)
        {
            throw new ApiException{
                ErrorMessage = "Username already exist",
                StatusCode = HttpStatusCode.Conflict};
        }

        _context.Users.Add(new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        });
        
        _context.SaveChangesAsync();
    }

    public AuthenticationResponse Authenticate(AuthenticationRequest request)
    {
        var user = _context.Users.SingleOrDefault(u => u.Username.Equals(request.Username));
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new ApiException{
                ErrorMessage = "Username or password is incorrect",
                StatusCode = HttpStatusCode.Unauthorized,
            };

        var jwtToken = _jwtUtils.GenerateUserToken(user);
        var refreshToken = _jwtUtils.GenerateRefreshToken();
        
        user.RefreshTokens.Add(refreshToken);
        
        RemoveStaleRefreshTokens(user);

        _context.SaveChanges();

        return new AuthenticationResponse
        {
            Username = user.Username,
            JwtToken = jwtToken,
            RefreshToken = refreshToken.Token,
        };
    }

    public AuthenticationResponse RefreshToken(string token)
    {
        var user = GetUserByRefreshToken(token);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);
        if (refreshToken == null) throw new ApiException { ErrorMessage = "Invalid token" };
        if (!user.RefreshTokens.Any(x=>x.Token.Equals(refreshToken.Token)) 
            || refreshToken.IsRevoked 
            || refreshToken.Expires <= DateTime.Now)
        {
            throw new ApiException { ErrorMessage = "Invalid token" };
        }
        
        var newRefreshToken = RotateRefreshToken(refreshToken);
        user.RefreshTokens.Add(newRefreshToken);
        
        _context.SaveChanges();

        var jwtToken = _jwtUtils.GenerateUserToken(user);

        return new AuthenticationResponse
        {
            Username = user.Username,
            JwtToken = jwtToken,
            RefreshToken = newRefreshToken.Token,
        };
    }

    public void RevokeToken(string token)
    {
        var user = GetUserByRefreshToken(token);
        var refreshToken = user.RefreshTokens.Single(x => x.Token.Equals(token));
        if (refreshToken.IsRevoked || refreshToken.Expires <= DateTime.Now) 
            throw new ApiException { ErrorMessage = "Invalid token" };
        refreshToken.IsRevoked = true;
        _context.Update(user);
        _context.SaveChanges();
    }

    public User GetCurrentUser()
    {
        if (_httpContextAccessor.HttpContext is null) throw new ApiException{ ErrorMessage = "HttpContext is null"};
        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
        if (userId is null) throw new ApiException {ErrorMessage = "Current user not found"};
        var user = _context.Users
            .Include(u => u.Todos)
            .SingleOrDefault(u => u.Id.Equals(int.Parse(userId)));
        if (user is null) throw new ApiException { ErrorMessage = $"No user with id {userId} found" };
        return user;
    }
    
    private RefreshToken RotateRefreshToken(RefreshToken refreshToken)
    {
        var newRefreshToken = _jwtUtils.GenerateRefreshToken();
        refreshToken.IsRevoked = true;
        return newRefreshToken;
    }
    
    private void RemoveStaleRefreshTokens(User user)
    {
        var tokens = user.RefreshTokens.Where(
            x => x.IsRevoked
                 || x.Expires <= DateTime.Now
                 
                 // Refresh Token TTL defined here
                 || x.TimeCreated.AddDays(3) <= DateTime.Now).ToList();
        foreach (var refreshToken in tokens)
        {
            user.RefreshTokens.Remove(refreshToken);
        }
    }
    
    private User GetUserByRefreshToken(string token)
    {
        var user = _context.Users
            .Include(user => user.RefreshTokens)
            .SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token.Equals(token)));
        if (user == null) throw new ApiException { ErrorMessage = "Invalid token" };
        return user;
    }

}