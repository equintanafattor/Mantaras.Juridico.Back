using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public class JuzgadoConfiguration : IEntityTypeConfiguration<Juzgado>
{
    public void Configure(EntityTypeBuilder<Juzgado> builder)
    {
        builder.ToTable("Juzgados");

        builder.HasKey(x => x.JuzgadoId);

        builder.Property(x => x.JuzgadoId).ValueGeneratedOnAdd();

        // Mismo largo que la antigua columna Expedientes.Juzgado: la migración
        // de los textos existentes no puede truncar ningún valor.
        builder.Property(x => x.Nombre).HasMaxLength(Juzgado.LargoMaximoNombre).IsRequired();

        builder.HasIndex(x => x.Nombre).IsUnique();

        builder.Property(x => x.FechaCreacion).IsRequired();

        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);

        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);

        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();
    }
}
