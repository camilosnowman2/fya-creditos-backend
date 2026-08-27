using FyaCreditos.Api.Common;
using FyaCreditos.Api.Data;
using FyaCreditos.Api.Dtos;
using FyaCreditos.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace FyaCreditos.Api.Controllers;

[ApiController]
[Route("api/creditos")]
public class CreditosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<CreditosController> _logger;

    public CreditosController(AppDbContext db, ILogger<CreditosController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuevo crédito y encola (en la misma transacción) su
    /// notificación por correo, que un BackgroundService envía en segundo
    /// plano — el registro nunca espera al SMTP.
    /// </summary>
    [HttpPost]
    [Authorize]
    [EnableRateLimiting("creditos-write")]
    public async Task<ActionResult<CreditoResponseDto>> Create([FromBody] CreditoCreateDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var credito = new Credito
        {
            Id = Guid.NewGuid(),
            NombreCliente = dto.NombreCliente.Trim(),
            Cedula = dto.Cedula.Trim(),
            ValorCredito = dto.ValorCredito,
            TasaInteres = dto.TasaInteres,
            PlazoMeses = dto.PlazoMeses,
            NombreComercial = dto.NombreComercial.Trim(),
            FechaRegistro = DateTimeOffset.UtcNow
        };

        var notificacion = new NotificacionCorreo
        {
            Id = Guid.NewGuid(),
            CreditoId = credito.Id,
            Estado = EstadoNotificacion.Pendiente,
            CreadoEn = DateTimeOffset.UtcNow
        };

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        _db.Creditos.Add(credito);
        _db.NotificacionesCorreo.Add(notificacion);
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        _logger.LogInformation("Crédito {CreditoId} registrado por comercial {Comercial}", credito.Id, credito.NombreComercial);

        return CreatedAtAction(nameof(GetById), new { id = credito.Id }, ToResponseDto(credito));
    }

    /// <summary>
    /// Lista créditos con filtros por nombre del cliente, cédula o
    /// comercial, y orden por fecha o valor del crédito.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PagedResult<CreditoResponseDto>>> GetAll(
        [FromQuery] CreditoQueryParametersDto query, CancellationToken ct)
    {
        var creditos = _db.Creditos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Nombre))
        {
            creditos = creditos.Where(c => EF.Functions.ILike(c.NombreCliente, $"%{query.Nombre.Trim()}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.Cedula))
        {
            creditos = creditos.Where(c => EF.Functions.ILike(c.Cedula, $"%{query.Cedula.Trim()}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.Comercial))
        {
            creditos = creditos.Where(c => EF.Functions.ILike(c.NombreComercial, $"%{query.Comercial.Trim()}%"));
        }

        creditos = (query.SortBy, query.Order) switch
        {
            (CreditoSortBy.Valor, SortOrder.Asc) => creditos.OrderBy(c => c.ValorCredito),
            (CreditoSortBy.Valor, SortOrder.Desc) => creditos.OrderByDescending(c => c.ValorCredito),
            (CreditoSortBy.Fecha, SortOrder.Asc) => creditos.OrderBy(c => c.FechaRegistro),
            _ => creditos.OrderByDescending(c => c.FechaRegistro)
        };

        var totalCount = await creditos.CountAsync(ct);

        // Nota: se proyecta con un inicializador de objeto inline (no llamando a
        // ToResponseDto) porque EF Core solo puede traducir a SQL una expresión
        // "new Dto { ... }" dentro de Select; una llamada a un método normal de
        // C# ahí lanzaría InvalidOperationException en tiempo de ejecución.
        var items = await creditos
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CreditoResponseDto
            {
                Id = c.Id,
                NombreCliente = c.NombreCliente,
                Cedula = c.Cedula,
                ValorCredito = c.ValorCredito,
                TasaInteres = c.TasaInteres,
                PlazoMeses = c.PlazoMeses,
                NombreComercial = c.NombreComercial,
                FechaRegistro = c.FechaRegistro
            })
            .ToListAsync(ct);

        return Ok(new PagedResult<CreditoResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        });
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<CreditoResponseDto>> GetById(Guid id, CancellationToken ct)
    {
        var credito = await _db.Creditos.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        if (credito is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Crédito no encontrado",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(ToResponseDto(credito));
    }

    private static CreditoResponseDto ToResponseDto(Credito c) => new()
    {
        Id = c.Id,
        NombreCliente = c.NombreCliente,
        Cedula = c.Cedula,
        ValorCredito = c.ValorCredito,
        TasaInteres = c.TasaInteres,
        PlazoMeses = c.PlazoMeses,
        NombreComercial = c.NombreComercial,
        FechaRegistro = c.FechaRegistro
    };
}
