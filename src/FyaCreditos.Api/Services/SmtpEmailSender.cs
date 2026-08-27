using FyaCreditos.Api.Models;
using FyaCreditos.Api.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FyaCreditos.Api.Services;

/// <summary>
/// Envía el correo por SMTP genérico usando MailKit. Cualquier proveedor
/// que hable SMTP funciona (Gmail con contraseña de aplicación, SendGrid,
/// Mailgun, un SMTP corporativo, Mailtrap para pruebas, etc.) — solo hay
/// que cambiar Smtp__Host/Port/User/Password. Ver README.md.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendCreditoNotificationAsync(Credito credito, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            throw new InvalidOperationException(
                "Smtp:Host no está configurado. Define Smtp__Host, Smtp__User, Smtp__Password " +
                "y Smtp__FromAddress como variables de entorno antes de correr la API.");
        }

        var message = new MimeMessage();
        var fromAddress = string.IsNullOrWhiteSpace(_options.FromAddress) ? _options.User : _options.FromAddress;
        message.From.Add(MailboxAddress.Parse($"\"{_options.FromName}\" <{fromAddress}>"));
        message.To.Add(MailboxAddress.Parse(_options.NotificationRecipient));
        message.Subject = $"Nuevo crédito registrado - {credito.NombreCliente}";

        message.Body = new TextPart("plain")
        {
            Text =
                $"Se registró un nuevo crédito.\n\n" +
                $"Nombre del cliente: {credito.NombreCliente}\n" +
                $"Valor del crédito: {credito.ValorCredito:N2}\n" +
                $"Comercial: {credito.NombreComercial}\n" +
                $"Fecha de registro: {credito.FechaRegistro:yyyy-MM-dd HH:mm:ss} UTC\n"
        };

        using var client = new SmtpClient();
        var socketOptions = _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

        await client.ConnectAsync(_options.Host, _options.Port, socketOptions, cancellationToken);
        if (!string.IsNullOrEmpty(_options.User))
        {
            await client.AuthenticateAsync(_options.User, _options.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation(
            "Correo de notificación enviado a {Recipient} para el crédito {CreditoId}",
            _options.NotificationRecipient, credito.Id);
    }
}
