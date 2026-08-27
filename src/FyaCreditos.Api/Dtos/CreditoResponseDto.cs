namespace FyaCreditos.Api.Dtos;

public class CreditoResponseDto
{
    public Guid Id { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public decimal ValorCredito { get; set; }
    public decimal TasaInteres { get; set; }
    public int PlazoMeses { get; set; }
    public string NombreComercial { get; set; } = string.Empty;
    public DateTimeOffset FechaRegistro { get; set; }
}
