using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class RecordatorioPredeterminadoTipoAgendaConfiguration
    : IEntityTypeConfiguration<RecordatorioPredeterminadoTipoAgenda>
{
    public void Configure(
        EntityTypeBuilder<RecordatorioPredeterminadoTipoAgenda> builder
    )
    {
        builder.ToTable(
            "RecordatoriosPredeterminadosTiposAgenda",
            tableBuilder =>
                tableBuilder.HasCheckConstraint(
                    "CK_RecordatoriosPredeterminadosTiposAgenda_Anticipacion",
                    "\"MinutosAnticipacion\" >= 0"
                )
        );

        builder.HasKey(x => x.RecordatorioPredeterminadoTipoAgendaId);
        builder.Property(x => x.RecordatorioPredeterminadoTipoAgendaId).ValueGeneratedOnAdd();
        builder.Property(x => x.BaseCalculo).HasConversion<int>().IsRequired();
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);
        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);
        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => new
        {
            x.TipoEntradaAgendaId,
            x.BaseCalculo,
            x.MinutosAnticipacion,
        }).IsUnique();

        builder
            .HasOne(x => x.TipoEntradaAgenda)
            .WithMany(x => x.RecordatoriosPredeterminados)
            .HasForeignKey(x => x.TipoEntradaAgendaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class RecordatorioPredeterminadoReglaVencimientoConfiguration
    : IEntityTypeConfiguration<RecordatorioPredeterminadoReglaVencimiento>
{
    public void Configure(
        EntityTypeBuilder<RecordatorioPredeterminadoReglaVencimiento> builder
    )
    {
        builder.ToTable(
            "RecordatoriosPredeterminadosReglasVencimiento",
            tableBuilder =>
                tableBuilder.HasCheckConstraint(
                    "CK_RecordatoriosPredeterminadosReglasVencimiento_Anticipacion",
                    "\"MinutosAnticipacion\" >= 0"
                )
        );

        builder.HasKey(x => x.RecordatorioPredeterminadoReglaVencimientoId);
        builder
            .Property(x => x.RecordatorioPredeterminadoReglaVencimientoId)
            .ValueGeneratedOnAdd();
        builder.Property(x => x.BaseCalculo).HasConversion<int>().IsRequired();
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);
        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);
        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => new
        {
            x.ReglaVencimientoId,
            x.BaseCalculo,
            x.MinutosAnticipacion,
        }).IsUnique();

        builder
            .HasOne(x => x.ReglaVencimiento)
            .WithMany(x => x.RecordatoriosPredeterminados)
            .HasForeignKey(x => x.ReglaVencimientoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
