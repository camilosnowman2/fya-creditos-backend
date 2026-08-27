using FyaCreditos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FyaCreditos.Api.Data;

/// <summary>
/// DbContext mapeado "database-first" contra el esquema creado por
/// db/init.sql (ver README). No se usan migraciones de EF Core en este
/// proyecto: el esquema y la semilla de datos viven como SQL plano para
/// que cualquiera pueda crear la base con "psql -f db/init.sql" sin
/// depender de las herramientas de EF Core.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Credito> Creditos => Set<Credito>();
    public DbSet<NotificacionCorreo> NotificacionesCorreo => Set<NotificacionCorreo>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Credito>(entity =>
        {
            entity.ToTable("creditos");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id");
            entity.Property(c => c.NombreCliente).HasColumnName("nombre_cliente").HasMaxLength(200).IsRequired();
            entity.Property(c => c.Cedula).HasColumnName("cedula").HasMaxLength(30).IsRequired();
            entity.Property(c => c.ValorCredito).HasColumnName("valor_credito").HasColumnType("numeric(14,2)");
            entity.Property(c => c.TasaInteres).HasColumnName("tasa_interes").HasColumnType("numeric(6,3)");
            entity.Property(c => c.PlazoMeses).HasColumnName("plazo_meses");
            entity.Property(c => c.NombreComercial).HasColumnName("nombre_comercial").HasMaxLength(200).IsRequired();
            entity.Property(c => c.FechaRegistro).HasColumnName("fecha_registro");

            entity.HasIndex(c => c.NombreCliente).HasDatabaseName("idx_creditos_nombre_cliente");
            entity.HasIndex(c => c.Cedula).HasDatabaseName("idx_creditos_cedula");
            entity.HasIndex(c => c.NombreComercial).HasDatabaseName("idx_creditos_nombre_comercial");
            entity.HasIndex(c => c.FechaRegistro).HasDatabaseName("idx_creditos_fecha_registro");
            entity.HasIndex(c => c.ValorCredito).HasDatabaseName("idx_creditos_valor_credito");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(u => u.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
            entity.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(200).IsRequired();
            entity.Property(u => u.CreadoEn).HasColumnName("creado_en");
            entity.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<NotificacionCorreo>(entity =>
        {
            entity.ToTable("notificaciones_correo");
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Id).HasColumnName("id");
            entity.Property(n => n.CreditoId).HasColumnName("credito_id");
            entity.Property(n => n.Estado).HasColumnName("estado").HasMaxLength(20);
            entity.Property(n => n.Intentos).HasColumnName("intentos");
            entity.Property(n => n.UltimoError).HasColumnName("ultimo_error");
            entity.Property(n => n.CreadoEn).HasColumnName("creado_en");
            entity.Property(n => n.EnviadoEn).HasColumnName("enviado_en");

            entity.HasIndex(n => n.Estado).HasDatabaseName("idx_notif_estado");

            entity.HasOne(n => n.Credito)
                  .WithMany(c => c.Notificaciones)
                  .HasForeignKey(n => n.CreditoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
