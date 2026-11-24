using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class DiaInhabilConfiguration
    : IEntityTypeConfiguration<DiaInhabil>
{
    public void Configure(EntityTypeBuilder<DiaInhabil> builder)
    {
        builder.ToTable("DiasInhabiles");

        builder.HasKey(x => x.DiaInhabilId);
        builder.Property(x => x.DiaInhabilId).ValueGeneratedOnAdd();
        builder.Property(x => x.Fecha).HasColumnType("date").IsRequired();
        builder.Property(x => x.Descripcion).HasMaxLength(300).IsRequired();
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);
        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);
        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();
        builder.Property(x => x.Origen)
            .HasConversion<int>()
            .HasDefaultValue(OrigenDiaInhabil.Manual)
            .IsRequired();
        builder.Property(x => x.Fuente).HasMaxLength(100);
        builder.Property(x => x.FechaFuente).HasColumnType("date");
        builder.Property(x => x.ModificadoManualmente).HasDefaultValue(false).IsRequired();

        builder.HasIndex(x => x.Fecha).IsUnique();
        builder.HasIndex(x => x.Activo);
        builder.HasIndex(x => x.FechaFuente);
    }
}
