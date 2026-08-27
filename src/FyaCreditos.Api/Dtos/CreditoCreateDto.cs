using System.ComponentModel.DataAnnotations;

namespace FyaCreditos.Api.Dtos;

/// <summary>Datos mínimos exigidos por el formulario de registro de créditos.</summary>
public class CreditoCreateDto
{
    [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre del cliente debe tener entre 2 y 200 caracteres.")]
    public string NombreCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cédula o ID es obligatoria.")]
    [StringLength(30, MinimumLength = 4, ErrorMessage = "La cédula debe tener entre 4 y 30 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9\-]+$", ErrorMessage = "La cédula solo puede contener letras, números y guiones.")]
    public string Cedula { get; set; } = string.Empty;

    [Range(0.01, 999999999999.99, ErrorMessage = "El valor del crédito debe ser mayor a 0.")]
    public decimal ValorCredito { get; set; }

    [Range(0, 100, ErrorMessage = "La tasa de interés debe estar entre 0 y 100.")]
    public decimal TasaInteres { get; set; }

    [Range(1, 600, ErrorMessage = "El plazo en meses debe estar entre 1 y 600.")]
    public int PlazoMeses { get; set; }

    [Required(ErrorMessage = "El nombre del comercial que registra el crédito es obligatorio.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre del comercial debe tener entre 2 y 200 caracteres.")]
    public string NombreComercial { get; set; } = string.Empty;
}
