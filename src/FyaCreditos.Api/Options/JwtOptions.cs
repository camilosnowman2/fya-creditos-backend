namespace FyaCreditos.Api.Options;

public class JwtOptions
{
    /// <summary>Clave secreta simétrica. Mínimo 32 caracteres. Nunca hardcodear en producción.</summary>
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "FyaCreditosApi";
    public string Audience { get; set; } = "FyaCreditosClient";
    public int ExpirationMinutes { get; set; } = 120;
}
