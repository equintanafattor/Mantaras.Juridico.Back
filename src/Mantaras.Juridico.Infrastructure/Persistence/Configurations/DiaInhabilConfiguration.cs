using Mantaras.Juridico.Domain.Entities;
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

        builder.HasIndex(x => x.Fecha).IsUnique();
        builder.HasIndex(x => x.Activo);
    }
}
