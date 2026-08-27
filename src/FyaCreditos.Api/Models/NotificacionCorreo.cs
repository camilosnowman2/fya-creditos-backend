namespace FyaCreditos.Api.Models;

public static class EstadoNotificacion
{
    public const string Pendiente = "PENDIENTE";
    public const string Enviado = "ENVIADO";
    public const string Error = "ERROR";
}

/// <summary>
/// Registro tipo "outbox": cada crédito nuevo crea una fila aquí en la misma
/// transacción de la escritura del crédito. Un BackgroundService la procesa
/// en segundo plano y envía el correo, así el registro del crédito nunca
/// espera a que el SMTP responda y no se pierde ningún envío si el proceso
/// se reinicia mientras hay notificaciones pendientes.
/// </summary>
public class NotificacionCorreo
{
    public Guid Id { get; set; }

    public Guid CreditoId { get; set; }

    public Credito? Credito { get; set; }

    public string Estado { get; set; } = EstadoNotificacion.Pendiente;

    public int Intentos { get; set; }

    public string? UltimoError { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public DateTimeOffset? EnviadoEn { get; set; }
}
