using FyaCreditos.Api.Models;

namespace FyaCreditos.Api.Services;

public interface IEmailSender
{
    /// <summary>Envía el correo de notificación de un nuevo crédito. Lanza si falla el envío.</summary>
    Task SendCreditoNotificationAsync(Credito credito, CancellationToken cancellationToken);
}
