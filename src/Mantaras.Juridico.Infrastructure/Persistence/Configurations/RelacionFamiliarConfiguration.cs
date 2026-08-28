using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class RelacionFamiliarConfiguration
    : IEntityTypeConfiguration<RelacionFamiliar>
{
    public void Configure(EntityTypeBuilder<RelacionFamiliar> builder)
    {
        builder.ToTable(
            "RelacionesFamiliares",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_RelacionesFamiliares_ClientesOrdenados",
                    "\"ClienteAId\" < \"ClienteBId\""
                );

                tableBuilder.HasCheckConstraint(
                    "CK_RelacionesFamiliares_ParentescoValido",
                    "\"ParentescoDeB\" BETWEEN 1 AND 10"
                );
            }
        );

        builder.HasKey(x => x.RelacionFamiliarId);

        builder.Property(x => x.RelacionFamiliarId)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ParentescoDeB)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Activo)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(x => x.FechaCreacion)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UsuarioCreacion)
            .HasMaxLength(100);

        builder.Property(x => x.FechaModificacion)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.UsuarioModificacion)
            .HasMaxLength(100);

        builder
            .HasIndex(x => new { x.ClienteAId, x.ClienteBId })
            .IsUnique();

        builder.HasIndex(x => x.ClienteBId);

        builder
            .HasOne(x => x.ClienteA)
            .WithMany(x => x.RelacionesFamiliaresComoA)
            .HasForeignKey(x => x.ClienteAId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.ClienteB)
            .WithMany(x => x.RelacionesFamiliaresComoB)
            .HasForeignKey(x => x.ClienteBId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}