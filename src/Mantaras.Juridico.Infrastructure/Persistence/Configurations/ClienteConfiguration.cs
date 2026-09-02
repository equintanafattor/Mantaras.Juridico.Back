using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(x => x.ClienteId);

        builder.Property(x => x.ClienteId).ValueGeneratedOnAdd();

        builder.Property(x => x.Nombre).HasMaxLength(100).IsRequired();

        builder.Property(x => x.Apellido).HasMaxLength(100).IsRequired();

        builder.Property(x => x.Dni).HasMaxLength(20);

        builder.Property(x => x.Cuil).HasMaxLength(20);

        builder.Property(x => x.ClaveSeguridadSocial).HasMaxLength(500);

        builder.Property(x => x.DerivadoPor).HasMaxLength(200);

        builder.Property(x => x.DerivadoPorTelefono).HasMaxLength(50);

        builder.Property(x => x.DerivadoPorEmail).HasMaxLength(200);

        builder.Property(x => x.Telefono).HasMaxLength(50);

        builder.Property(x => x.Email).HasMaxLength(200);

        builder.Property(x => x.Domicilio).HasMaxLength(300);

        builder.Property(x => x.Localidad).HasMaxLength(150);

        builder.Property(x => x.Provincia).HasMaxLength(150);

        builder.Property(x => x.FechaCreacion).IsRequired();
        
        builder.Property(cliente => cliente.FechaNacimiento).HasColumnType("date");

        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);

        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);

        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.Dni).IsUnique();

        builder.HasIndex(x => x.Cuil).IsUnique();

        builder.HasIndex(x => new { x.Apellido, x.Nombre });
    }
}
