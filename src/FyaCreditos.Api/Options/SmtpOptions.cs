namespace FyaCreditos.Api.Options;

/// <summary>
/// Configuración del servidor SMTP. Se llena desde variables de entorno
/// (Smtp__Host, Smtp__Port, etc.) o desde appsettings.Development.json
/// (no versionado). Nunca debe contener credenciales reales en el repo.
/// </summary>
public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "Fya Social Capital - Créditos";
    public bool UseSsl { get; set; } = true;

    /// <summary>Destinatario fijo pedido por el examen técnico.</summary>
    public string NotificationRecipient { get; set; } = "fyasocialcapital@gmail.com";
}
