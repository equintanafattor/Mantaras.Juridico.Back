using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class EntradaAgendaConfiguration
    : IEntityTypeConfiguration<EntradaAgenda>
{
    public void Configure(EntityTypeBuilder<EntradaAgenda> builder)
    {
        builder.ToTable(
            "EntradasAgenda",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_EntradasAgenda_Fechas",
                    "\"FechaFin\" IS NULL OR \"FechaFin\" >= \"FechaInicio\""
                );
                tableBuilder.HasCheckConstraint(
                    "CK_EntradasAgenda_HoraFin",
                    "\"HoraFin\" IS NULL OR \"FechaFin\" IS NOT NULL"
                );
                tableBuilder.HasCheckConstraint(
                    "CK_EntradasAgenda_HoraVencimiento",
                    "\"HoraVencimiento\" IS NULL OR \"FechaVencimiento\" IS NOT NULL"
                );
            }
        );

        builder.HasKey(x => x.EntradaAgendaId);
        builder.Property(x => x.EntradaAgendaId).ValueGeneratedOnAdd();
        builder.Property(x => x.Titulo).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Descripcion).HasMaxLength(4000);
        builder.Property(x => x.Estado).HasConversion<int>().IsRequired();
        builder.Property(x => x.Prioridad).HasConversion<int>().IsRequired();
        builder.Property(x => x.FechaInicio).HasColumnType("date").IsRequired();
        builder.Property(x => x.HoraInicio).HasColumnType("time without time zone");
        builder.Property(x => x.FechaFin).HasColumnType("date");
        builder.Property(x => x.HoraFin).HasColumnType("time without time zone");
        builder.Property(x => x.FechaVencimiento).HasColumnType("date");
        builder.Property(x => x.HoraVencimiento).HasColumnType("time without time zone");
        builder
            .Property(x => x.ZonaHoraria)
            .HasMaxLength(100)
            .HasDefaultValue(EntradaAgenda.ZonaHorariaPredeterminada)
            .IsRequired();
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);
        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);
        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => new { x.FechaInicio, x.Estado });
        builder.HasIndex(x => new { x.FechaVencimiento, x.Estado });
        builder.HasIndex(x => x.TipoEntradaAgendaId);
        builder.HasIndex(x => x.RecurrenciaAgendaId);
        builder.HasIndex(x => x.Activo);

        builder
            .HasOne(x => x.TipoEntrada)
            .WithMany(x => x.Entradas)
            .HasForeignKey(x => x.TipoEntradaAgendaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Recurrencia)
            .WithMany(x => x.Entradas)
            .HasForeignKey(x => x.RecurrenciaAgendaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
