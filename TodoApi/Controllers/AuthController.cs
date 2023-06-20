
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Dtos;
using TodoApi.Exceptions;
using TodoApi.Services;

namespace TodoApi.Controllers;
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpPost("login")]
    public IActionResult Authenticate(AuthenticationRequest request)
    {
        var response = _userService.Authenticate(request);
        AppendRefreshTokenCookie(response.RefreshToken);
        return Ok(response);
    }
    
    [HttpPost("register")]
    public IActionResult Register(AuthenticationRequest request)
    {
        _userService.Register(request);
        return Ok();
    }
    
    [HttpPost("refresh")]
    public IActionResult RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"] ?? "";
        var response = _userService.RefreshToken(refreshToken);
        AppendRefreshTokenCookie(response.RefreshToken);
        return Ok(response);
    }
    
    [HttpPost("revoke")]
    public IActionResult RevokeToken(RevokeTokenRequest request)
    {
        var token = request.Token ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(token))
            throw new ApiException { ErrorMessage = "Token is required" };

        _userService.RevokeToken(token);
        return Ok(new { message = "Token revoked" });
    }
    
    private void AppendRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }
}