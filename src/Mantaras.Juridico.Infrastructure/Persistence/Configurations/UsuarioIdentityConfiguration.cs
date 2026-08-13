using Mantaras.Juridico.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class UsuarioIdentityConfiguration : IEntityTypeConfiguration<UsuarioIdentity>
{
    public void Configure(EntityTypeBuilder<UsuarioIdentity> builder)
    {
        builder.Property(x => x.Nombre).HasMaxLength(150).IsRequired();

        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();

        builder.Property(x => x.NormalizedEmail).HasMaxLength(256).IsRequired();

        builder.HasIndex(x => x.NormalizedEmail).HasDatabaseName("EmailIndex").IsUnique();

        builder.Property(x => x.Activo).HasDefaultValue(true);

        builder.Property(x => x.FechaCreacion).IsRequired();
    }
}
