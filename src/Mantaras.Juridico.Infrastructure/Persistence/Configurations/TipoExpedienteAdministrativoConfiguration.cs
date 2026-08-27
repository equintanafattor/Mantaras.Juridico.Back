using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public class TipoExpedienteAdministrativoConfiguration
    : IEntityTypeConfiguration<TipoExpedienteAdministrativo>
{
    public void Configure(EntityTypeBuilder<TipoExpedienteAdministrativo> builder)
    {
        builder.ToTable("TiposExpedienteAdministrativo");

        builder.HasKey(x => x.TipoExpedienteAdministrativoId);

        builder.Property(x => x.TipoExpedienteAdministrativoId).ValueGeneratedOnAdd();

        builder.Property(x => x.Nombre).HasMaxLength(150).IsRequired();

        builder.HasIndex(x => x.Nombre).IsUnique();

        builder.Property(x => x.FechaCreacion).IsRequired();

        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);

        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);

        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();
    }
}
