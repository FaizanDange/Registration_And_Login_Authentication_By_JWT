using AuthJwtApi.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto registerDto)
    {
        try
        {
            var user = await _authService.Register(registerDto);
            return Ok(new { user.Id, user.Username, user.Email });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        try
        {
            var tokenDto = await _authService.Login(loginDto);
            return Ok(tokenDto);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Invalid login attempt");
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        try
        {
            var tokenDto = await _authService.RefreshToken(refreshToken);
            return Ok(tokenDto);
        }
        catch (SecurityTokenException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("revoke-token")]
    [Authorize] // Require authentication to revoke a token
    public async Task<IActionResult> RevokeToken([FromBody] string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest("Refresh token is required");

        try
        {
            await _authService.RevokeToken(refreshToken);
            return NoContent(); // 204 No Content (successful request)
        }
        catch (SecurityTokenException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}