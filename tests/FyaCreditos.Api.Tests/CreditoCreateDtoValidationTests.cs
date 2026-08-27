using System.ComponentModel.DataAnnotations;
using FyaCreditos.Api.Dtos;
using Xunit;

namespace FyaCreditos.Api.Tests;

public class CreditoCreateDtoValidationTests
{
    private static List<ValidationResult> Validate(CreditoCreateDto dto)
    {
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
        return results;
    }

    private static CreditoCreateDto CreditoValido() => new()
    {
        NombreCliente = "Pepito Perez",
        Cedula = "1000000001",
        ValorCredito = 7_800_000m,
        TasaInteres = 2m,
        PlazoMeses = 10,
        NombreComercial = "Comercial Uno"
    };

    [Fact]
    public void Credito_valido_no_produce_errores()
    {
        var dto = CreditoValido();
        var errores = Validate(dto);
        Assert.Empty(errores);
    }

    [Fact]
    public void Valor_credito_negativo_es_invalido()
    {
        var dto = CreditoValido();
        dto.ValorCredito = -100;

        var errores = Validate(dto);

        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(CreditoCreateDto.ValorCredito)));
    }

    [Fact]
    public void Plazo_en_meses_cero_es_invalido()
    {
        var dto = CreditoValido();
        dto.PlazoMeses = 0;

        var errores = Validate(dto);

        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(CreditoCreateDto.PlazoMeses)));
    }

    [Fact]
    public void Cedula_con_caracteres_invalidos_es_rechazada()
    {
        var dto = CreditoValido();
        dto.Cedula = "1000 000; DROP TABLE creditos;";

        var errores = Validate(dto);

        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(CreditoCreateDto.Cedula)));
    }

    [Fact]
    public void Nombre_cliente_vacio_es_invalido()
    {
        var dto = CreditoValido();
        dto.NombreCliente = "";

        var errores = Validate(dto);

        Assert.Contains(errores, e => e.MemberNames.Contains(nameof(CreditoCreateDto.NombreCliente)));
    }
}
