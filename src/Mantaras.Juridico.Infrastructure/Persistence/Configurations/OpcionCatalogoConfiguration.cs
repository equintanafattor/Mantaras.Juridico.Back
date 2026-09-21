using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class OpcionCatalogoConfiguration
    : IEntityTypeConfiguration<OpcionCatalogo>
{
    public void Configure(EntityTypeBuilder<OpcionCatalogo> builder)
    {
        builder.ToTable("OpcionesCatalogo");

        builder.HasKey(x => x.OpcionCatalogoId);
        builder.Property(x => x.OpcionCatalogoId).ValueGeneratedOnAdd();
        builder.Property(x => x.Tipo).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);
        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);
        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => new { x.Tipo, x.Nombre }).IsUnique();
        builder.HasIndex(x => new { x.Tipo, x.Activo });

        builder.HasData(
            CrearFase(1, "Preadministrativa"),
            CrearFase(2, "Juicio"),
            CrearFase(3, "Postjuicio")
        );
    }

    private static OpcionCatalogo CrearFase(long id, string nombre) => new()
    {
        OpcionCatalogoId = id,
        Tipo = "fases-internas",
        Nombre = nombre,
        Activo = true,
        FechaCreacion = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UsuarioCreacion = "Sistema",
    };
}
