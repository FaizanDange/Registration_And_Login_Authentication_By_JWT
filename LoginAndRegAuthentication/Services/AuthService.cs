using AuthJwtApi.Data;
using AuthJwtApi.Models.DTOs;
using AuthJwtApi.Models.Entities;
//using LoginAndRegAuthentication.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public AuthService(AppDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<User> Register(RegisterDto registerDto)
    {
        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            throw new ArgumentException("Email already exists");

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        // Create new user
        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = passwordHash
        };

        // Find the role in the database
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == registerDto.Role);

        if (role == null)
            throw new ArgumentException("Invalid role. Allowed roles: User, Admin");

        // Assign the role dynamically
        user.UserRoles = new List<UserRole> { new UserRole { RoleId = role.Id } };

        // Save user to database
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }


    public async Task<TokenDto> Login(LoginDto loginDto)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var token = _jwtService.GenerateJwtToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        await _context.SaveChangesAsync();

        return new TokenDto
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(_jwtService.GetTokenExpirationMinutes())
        };
    }

    public async Task<TokenDto> RefreshToken(string refreshToken)
    {
        // Find the user associated with the refresh token
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken));

        if (user == null)
            throw new SecurityTokenException("Invalid refresh token");

        // Find the specific refresh token
        var existingRefreshToken = user.RefreshTokens.Single(rt => rt.Token == refreshToken);

        // Check if the refresh token is active
        if (!existingRefreshToken.IsActive)
            throw new SecurityTokenException("Refresh token is no longer active");

        // Generate a new access token
        var newAccessToken = _jwtService.GenerateJwtToken(user);

        // Generate a new refresh token
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        // Revoke the existing refresh token
        existingRefreshToken.Revoked = DateTime.UtcNow;

        // Add the new refresh token to the user
        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            Expires = DateTime.UtcNow.AddDays(7)
        });

        // Save changes to the database
        await _context.SaveChangesAsync();

        // Return the new tokens
        return new TokenDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(_jwtService.GetTokenExpirationMinutes())
        };
    }

    public async Task RevokeToken(string refreshToken)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.Token == refreshToken));

        if (user == null)
            throw new SecurityTokenException("Invalid refresh token");

        var existingRefreshToken = user.RefreshTokens.SingleOrDefault(rt => rt.Token == refreshToken);

        if (existingRefreshToken == null)
            throw new SecurityTokenException("Refresh token not found");

        if (!existingRefreshToken.IsActive)
            throw new SecurityTokenException("Refresh token is no longer active");

        existingRefreshToken.Revoked = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

}