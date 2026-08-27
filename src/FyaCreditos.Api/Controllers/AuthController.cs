using FyaCreditos.Api.Dtos;
using FyaCreditos.Api.Options;
using FyaCreditos.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace FyaCreditos.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthCredentialsOptions _authOptions;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IOptions<AuthCredentialsOptions> authOptions, IJwtTokenService jwtTokenService)
    {
        _authOptions = authOptions.Value;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// Login simple de un único usuario administrador (configurado por
    /// variables de entorno) que devuelve un JWT para poder registrar
    /// créditos. Ver README.md para configurar el usuario/clave.
    /// </summary>
    [HttpPost("login")]
    public ActionResult<LoginResponseDto> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var credencialesValidas =
            !string.IsNullOrEmpty(_authOptions.PasswordHash) &&
            string.Equals(dto.Username, _authOptions.Username, StringComparison.Ordinal) &&
            SafeVerify(dto.Password, _authOptions.PasswordHash);

        if (!credencialesValidas)
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
        catch (BCrypt.Net.SaltParseException)
        {
            // AuthCredentials:PasswordHash mal configurado (no es un hash BCrypt válido).
            return false;
        }
    }
}
