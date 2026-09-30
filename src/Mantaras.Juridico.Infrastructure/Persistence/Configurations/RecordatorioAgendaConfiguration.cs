using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mantaras.Juridico.Infrastructure.Persistence.Configurations;

public sealed class RecordatorioAgendaConfiguration
    : IEntityTypeConfiguration<RecordatorioAgenda>
{
    public void Configure(EntityTypeBuilder<RecordatorioAgenda> builder)
    {
        builder.ToTable("RecordatoriosAgenda");

        builder.HasKey(x => x.RecordatorioAgendaId);
        builder.Property(x => x.RecordatorioAgendaId).ValueGeneratedOnAdd();
        builder.Property(x => x.Canal).HasConversion<int>().IsRequired();
        builder
            .Property(x => x.FechaProgramadaUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(x => x.FechaAtendidoUtc).HasColumnType("timestamp with time zone");
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.UsuarioCreacion).HasMaxLength(100);
        builder.Property(x => x.UsuarioModificacion).HasMaxLength(100);
        builder.Property(x => x.Activo).HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => new { x.FechaProgramadaUtc, x.Atendido, x.Activo });
        builder.HasIndex(x => x.UsuarioAtendioId);

        builder
            .HasOne(x => x.EntradaAgenda)
            .WithMany(x => x.Recordatorios)
            .HasForeignKey(x => x.EntradaAgendaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<UsuarioIdentity>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioAtendioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
