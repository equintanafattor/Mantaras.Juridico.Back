using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public class CasoConfiguration : IEntityTypeConfiguration<Caso>
{
    public void Configure(EntityTypeBuilder<Caso> builder)
    {
        builder.ToTable("Casos");

        builder.HasKey(x => x.CasoId);

        builder.Property(x => x.Titulo).HasMaxLength(300).IsRequired();

        builder.Property(x => x.FaseInterna).HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(x => x.TipoTramite).HasMaxLength(200);

        builder.Property(x => x.Observaciones).HasMaxLength(2000);

        builder.Property(x => x.Activo).HasDefaultValue(true);

        builder.HasIndex(x => x.Titulo);

        builder.HasIndex(x => x.FaseInterna);

        builder
            .HasMany(x => x.Expedientes)
            .WithOne(x => x.Caso)
            .HasForeignKey(x => x.CasoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);

        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);
    }
}
