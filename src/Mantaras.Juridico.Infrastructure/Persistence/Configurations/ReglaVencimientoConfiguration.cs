using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class ReglaVencimientoConfiguration
    : IEntityTypeConfiguration<ReglaVencimiento>
{
    public void Configure(EntityTypeBuilder<ReglaVencimiento> builder)
    {
        builder.ToTable(
            "ReglasVencimiento",
            tableBuilder =>
                tableBuilder.HasCheckConstraint(
                    "CK_ReglasVencimiento_CantidadDias",
                    "\"CantidadDias\" >= 0"
                )
        );

        builder.HasKey(x => x.ReglaVencimientoId);
        builder.Property(x => x.ReglaVencimientoId).ValueGeneratedOnAdd();
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Descripcion).HasMaxLength(1000);
        builder.Property(x => x.TipoComputo).HasConversion<int>().IsRequired();
        builder.Property(x => x.SentidoCalculo).HasConversion<int>().IsRequired();
        builder.Property(x => x.PrioridadGenerada).HasConversion<int>().IsRequired();
        builder.Property(x => x.HoraSugerida).HasColumnType("time without time zone");
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);
        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);
        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.Nombre).IsUnique();
        builder.HasIndex(x => new { x.TipoEntradaAgendaId, x.Activo });

        builder
            .HasOne(x => x.TipoEntrada)
            .WithMany(x => x.ReglasVencimiento)
            .HasForeignKey(x => x.TipoEntradaAgendaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
