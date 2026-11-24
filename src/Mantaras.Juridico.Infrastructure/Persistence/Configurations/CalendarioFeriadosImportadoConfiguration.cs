using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class CalendarioFeriadosImportadoConfiguration
    : IEntityTypeConfiguration<CalendarioFeriadosImportado>
{
    public void Configure(EntityTypeBuilder<CalendarioFeriadosImportado> builder)
    {
        builder.ToTable("CalendariosFeriadosImportados");

        builder.HasKey(x => x.Anio);
        builder.Property(x => x.Anio).ValueGeneratedNever();
        builder.Property(x => x.Fuente).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UrlFuente).HasMaxLength(500).IsRequired();
        builder.Property(x => x.FechaPrimeraSincronizacion).IsRequired();
        builder.Property(x => x.FechaSincronizacion).IsRequired();
        builder.Property(x => x.UsuarioSincronizacion).HasMaxLength(100);
        builder.Property(x => x.UltimaClaveConfirmacion).HasMaxLength(64).IsRequired();
    }
}
