using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public class TipoBeneficioConfiguration : IEntityTypeConfiguration<TipoBeneficio>
{
    public void Configure(EntityTypeBuilder<TipoBeneficio> builder)
    {
        builder.ToTable("TiposBeneficio");

        builder.HasKey(x => x.TipoBeneficioId);

        builder.Property(x => x.TipoBeneficioId).ValueGeneratedOnAdd();

        builder.Property(x => x.Nombre).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => x.Nombre).IsUnique();

        builder.Property(x => x.FechaCreacion).IsRequired();

        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);

        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);

        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();
    }
}
