using AuthJwtApi.Models.DTOs;
using AuthJwtApi.Models.Entities;

public interface IAuthService
{
    Task<User> Register(RegisterDto registerDto);
    Task<TokenDto> Login(LoginDto loginDto);
    Task<TokenDto> RefreshToken(string refreshToken);
    Task RevokeToken(string refreshToken);
}