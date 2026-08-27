namespace FyaCreditos.Api.Services;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAtUtc) GenerateToken(string username);
}
