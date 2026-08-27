namespace FyaCreditos.Api.Models;

/// <summary>
/// Representa un crédito registrado por un comercial.
/// Mapeada 1:1 contra la tabla "creditos" definida en db/init.sql.
/// </summary>
public class Credito
{
    public Guid Id { get; set; }

    public string NombreCliente { get; set; } = string.Empty;

    public string Cedula { get; set; } = string.Empty;

    public decimal ValorCredito { get; set; }

    /// <summary>Tasa de interés expresada como porcentaje (ej: 2.00 = 2%).</summary>
    public decimal TasaInteres { get; set; }

    public int PlazoMeses { get; set; }

    public string NombreComercial { get; set; } = string.Empty;

    public DateTimeOffset FechaRegistro { get; set; }

    public ICollection<NotificacionCorreo> Notificaciones { get; set; } = new List<NotificacionCorreo>();
}
