using FyaCreditos.Api.Data;
using FyaCreditos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FyaCreditos.Api.Services;

/// <summary>
/// Procesa en segundo plano la tabla "notificaciones_correo" (patrón outbox).
/// Cada crédito nuevo ya quedó guardado con su notificación en estado
/// PENDIENTE dentro de la misma transacción; este servicio corre cada
/// pocos segundos, toma las pendientes y envía el correo. Si el envío
/// falla (SMTP mal configurado, red caída, etc.) incrementa el contador
/// de intentos y reintenta más adelante, hasta un máximo de intentos.
/// </summary>
public class EmailOutboxBackgroundService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int MaxIntentos = 5;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailOutboxBackgroundService> _logger;

    public EmailOutboxBackgroundService(IServiceScopeFactory scopeFactory, ILogger<EmailOutboxBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ProcessPendingAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error inesperado procesando la cola de notificaciones de correo.");
            }
        }
    }

    private async Task ProcessPendingAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var pendientes = await db.NotificacionesCorreo
            .Include(n => n.Credito)
            .Where(n => n.Estado == EstadoNotificacion.Pendiente && n.Intentos < MaxIntentos)
            .OrderBy(n => n.CreadoEn)
            .Take(20)
            .ToListAsync(ct);

        if (pendientes.Count == 0)
        {
            return;
        }

        foreach (var notificacion in pendientes)
        {
            if (notificacion.Credito is null)
            {
                notificacion.Estado = EstadoNotificacion.Error;
                notificacion.UltimoError = "El crédito asociado ya no existe.";
                continue;
            }

            try
            {
                await emailSender.SendCreditoNotificationAsync(notificacion.Credito, ct);
                notificacion.Estado = EstadoNotificacion.Enviado;
                notificacion.EnviadoEn = DateTimeOffset.UtcNow;
                notificacion.UltimoError = null;
            }
            catch (Exception ex)
            {
                notificacion.Intentos += 1;
                notificacion.UltimoError = ex.Message;
                notificacion.Estado = notificacion.Intentos >= MaxIntentos
                    ? EstadoNotificacion.Error
                    : EstadoNotificacion.Pendiente;

                _logger.LogWarning(ex,
                    "Intento {Intentos} fallido enviando correo para el crédito {CreditoId}",
                    notificacion.Intentos, notificacion.CreditoId);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
