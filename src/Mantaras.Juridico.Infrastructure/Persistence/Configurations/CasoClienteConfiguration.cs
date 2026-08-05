using Mantaras.Juridico.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public class CasoClienteConfiguration : IEntityTypeConfiguration<CasoCliente>
{
    public void Configure(EntityTypeBuilder<CasoCliente> builder)
    {
        builder.ToTable("CasosClientes");

        builder.HasKey(x => new { x.CasoId, x.ClienteId });

        builder
            .Property(x => x.TipoParticipacion)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.EsPrincipal).IsRequired();

        builder
            .HasOne(x => x.Caso)
            .WithMany(x => x.Clientes)
            .HasForeignKey(x => x.CasoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Cliente)
            .WithMany(x => x.Casos)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ClienteId);
    }
}
