namespace FyaCreditos.Api.Options;

/// <summary>
/// Credenciales del único usuario administrador que puede registrar créditos.
/// La contraseña se guarda como hash BCrypt, nunca en texto plano.
/// Genera tu propio hash con: python3 tools/generate_password_hash.py
/// (ver README.md).
/// </summary>
public class AuthCredentialsOptions
{
    public string Username { get; set; } = "admin";

    /// <summary>Hash BCrypt de la contraseña (no la contraseña en texto plano).</summary>
    public string PasswordHash { get; set; } = string.Empty;
}
