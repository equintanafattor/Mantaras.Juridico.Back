using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class TipoEntradaAgendaConfiguration
    : IEntityTypeConfiguration<TipoEntradaAgenda>
{
    public void Configure(EntityTypeBuilder<TipoEntradaAgenda> builder)
    {
        builder.ToTable("TiposEntradaAgenda");

        builder.HasKey(x => x.TipoEntradaAgendaId);
        builder.Property(x => x.TipoEntradaAgendaId).ValueGeneratedOnAdd();
        builder.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Descripcion).HasMaxLength(500);
        builder.Property(x => x.Color).HasMaxLength(30);
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);
        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);
        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.Nombre).IsUnique();
        builder.HasIndex(x => x.Activo);

        builder.HasData(
            Crear(1, "Tarea interna", "blue"),
            Crear(2, "Vencimiento procesal", "red"),
            Crear(3, "Audiencia o turno", "purple"),
            Crear(4, "Recordatorio general", "yellow"),
            Crear(5, "Reunión o llamado", "green")
        );
    }

    private static TipoEntradaAgenda Crear(long id, string nombre, string color) => new()
    {
        TipoEntradaAgendaId = id,
        Nombre = nombre,
        Color = color,
        FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UsuarioCreacion = "Sistema",
        Activo = true,
    };
}
