using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class RecurrenciaAgendaConfiguration
    : IEntityTypeConfiguration<RecurrenciaAgenda>
{
    public void Configure(EntityTypeBuilder<RecurrenciaAgenda> builder)
    {
        builder.ToTable(
            "RecurrenciasAgenda",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_RecurrenciasAgenda_Intervalo",
                    "\"Intervalo\" > 0"
                );
                tableBuilder.HasCheckConstraint(
                    "CK_RecurrenciasAgenda_DiasSemana",
                    "\"DiasSemana\" IS NULL OR (\"DiasSemana\" BETWEEN 1 AND 127)"
                );
                tableBuilder.HasCheckConstraint(
                    "CK_RecurrenciasAgenda_DiaMes",
                    "\"DiaMes\" IS NULL OR (\"DiaMes\" BETWEEN 1 AND 31)"
                );
                tableBuilder.HasCheckConstraint(
                    "CK_RecurrenciasAgenda_MesAnio",
                    "\"MesAnio\" IS NULL OR (\"MesAnio\" BETWEEN 1 AND 12)"
                );
                tableBuilder.HasCheckConstraint(
                    "CK_RecurrenciasAgenda_Fechas",
                    "\"FechaFin\" IS NULL OR \"FechaFin\" >= \"FechaInicio\""
                );
                tableBuilder.HasCheckConstraint(
                    "CK_RecurrenciasAgenda_MaximoOcurrencias",
                    "\"MaximoOcurrencias\" IS NULL OR \"MaximoOcurrencias\" > 0"
                );
            }
        );

        builder.HasKey(x => x.RecurrenciaAgendaId);
        builder.Property(x => x.RecurrenciaAgendaId).ValueGeneratedOnAdd();
        builder.Property(x => x.Frecuencia).HasConversion<int>().IsRequired();
        builder.Property(x => x.FechaInicio).HasColumnType("date").IsRequired();
        builder.Property(x => x.FechaFin).HasColumnType("date");
        builder.Property(x => x.ProximaFecha).HasColumnType("date");
        builder
            .Property(x => x.ZonaHoraria)
            .HasMaxLength(100)
            .HasDefaultValue(EntradaAgenda.ZonaHorariaPredeterminada)
            .IsRequired();
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);
        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);
        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => new { x.ProximaFecha, x.Activo });
    }
}
