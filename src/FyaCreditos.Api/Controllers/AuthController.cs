using FyaCreditos.Api.Data;
using FyaCreditos.Api.Dtos;
using FyaCreditos.Api.Models;
using FyaCreditos.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace FyaCreditos.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(AppDbContext db, IJwtTokenService jwtTokenService)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var existe = await _db.Usuarios.AnyAsync(u => u.Username == dto.Username);
        if (existe)
        {
            return Conflict(new ProblemDetails
            {
                Title = "El usuario ya existe",
                Status = StatusCodes.Status409Conflict
            });
        }

        var usuario = new Usuario
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 11)
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        var (token, expiresAtUtc) = _jwtTokenService.GenerateToken(dto.Username);
        return Ok(new LoginResponseDto { Token = token, ExpiresAtUtc = expiresAtUtc });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (usuario == null || !SafeVerify(dto.Password, usuario.PasswordHash))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Credenciales inválidas",
                Status = StatusCodes.Status401Unauthorized
            });
        }

        var (token, expiresAtUtc) = _jwtTokenService.GenerateToken(dto.Username);
        return Ok(new LoginResponseDto { Token = token, ExpiresAtUtc = expiresAtUtc });
    }

    private static bool SafeVerify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
