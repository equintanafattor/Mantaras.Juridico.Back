using Mantaras.Juridico.Application.Features.DiasInhabiles.Importacion;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Tests.DiasInhabiles;

public class PlanificadorImportacionFeriadosTests
{
    private static readonly PublicacionFeriadosNacionales Publicacion2026 = Fixtures.Publicacion(2026);

    [Fact]
    public void Separa_feriados_nacionales_de_los_que_requieren_revision()
    {
        var plan = PlanificadorImportacionFeriados.Construir(Publicacion2026, Array.Empty<DiaInhabil>());

        var nacionales = plan.FeriadosNacionales.ToArray();
        var revision = plan.RequierenRevision.ToArray();

        // 2026: 31 fechas propias del año (el 2/4 y el 3/4 vienen repetidos);
        // 17 con feriado nacional y 14 solo no laborables, turísticas o especiales.
        Assert.Equal(17, nacionales.Length);
        Assert.Equal(14, revision.Length);
        Assert.All(nacionales, x => Assert.Equal(EstadoImportacionFeriado.Nuevo, x.Estado));
        Assert.Contains(revision, x => x.Fecha == new DateOnly(2026, 3, 23)); // turístico
        Assert.Contains(revision, x => x.Fecha == new DateOnly(2026, 11, 10)); // especial regional
        Assert.Contains(revision, x => x.Fecha == new DateOnly(2026, 4, 24)); // no laborable
        Assert.DoesNotContain(nacionales, x => x.Fecha == new DateOnly(2026, 3, 23));
        Assert.Equal(new DateOnly(2027, 1, 1), Assert.Single(plan.Omitidos).Registro.Fecha);
    }

    [Fact]
    public void Agrupa_fechas_repetidas_y_prioriza_el_feriado_nacional()
    {
        var plan = PlanificadorImportacionFeriados.Construir(Publicacion2026, Array.Empty<DiaInhabil>());

        var dosDeAbril = Assert.Single(plan.Items, x => x.Fecha == new DateOnly(2026, 4, 2));

        Assert.Equal(CategoriaImportacionFeriado.FeriadoNacional, dosDeAbril.Categoria);
        Assert.Equal("Día del Veterano y de los Caídos en la Guerra de Malvinas", dosDeAbril.Descripcion);
        Assert.Equal(3, dosDeAbril.RegistrosFuente.Count);
    }

    [Fact]
    public void Normaliza_espacios_de_la_descripcion()
    {
        var plan = PlanificadorImportacionFeriados.Construir(Publicacion2026, Array.Empty<DiaInhabil>());

        Assert.Equal("Día de la Raza", plan.Items.Single(x => x.Fecha == new DateOnly(2026, 10, 12)).Descripcion);
    }

    [Fact]
    public void Clasifica_contra_los_registros_existentes()
    {
        var existentes = new[]
        {
            // Manual activo con otra descripción: se conserva.
            Dia(1, new DateOnly(2026, 5, 1), "Feriado manual", OrigenDiaInhabil.Manual),
            // Desactivado por el usuario: conflicto, no se reactiva.
            Dia(2, new DateOnly(2026, 5, 25), "Día de la Revolución de Mayo", OrigenDiaInhabil.FeriadoNacional, activo: false, modificado: true),
            // Importado sin cambios manuales y la fuente cambió la descripción.
            Dia(3, new DateOnly(2026, 7, 9), "Independencia", OrigenDiaInhabil.FeriadoNacional),
            // Importado movido a mano del 17/8 al 18/8.
            Dia(4, new DateOnly(2026, 8, 18), "San Martín", OrigenDiaInhabil.FeriadoNacional, modificado: true, fechaFuente: new DateOnly(2026, 8, 17)),
            // Importado que la fuente ya no publica.
            Dia(5, new DateOnly(2026, 6, 1), "Feriado retirado", OrigenDiaInhabil.FeriadoNacional),
        };

        var plan = PlanificadorImportacionFeriados.Construir(Publicacion2026, existentes);

        Assert.Equal(EstadoImportacionFeriado.Existente, Estado(plan, 5, 1));
        Assert.Equal(EstadoImportacionFeriado.Conflicto, Estado(plan, 5, 25));
        Assert.Equal(EstadoImportacionFeriado.Actualizable, Estado(plan, 7, 9));
        Assert.Equal(EstadoImportacionFeriado.Conflicto, Estado(plan, 8, 17));
        Assert.Equal(5, Assert.Single(plan.AusentesEnFuente).DiaInhabilId);
    }

    [Fact]
    public void La_huella_cambia_si_cambia_un_registro_afectado()
    {
        var existente = Dia(1, new DateOnly(2026, 5, 1), "Día del Trabajador", OrigenDiaInhabil.Manual);
        var antes = PlanificadorImportacionFeriados.Construir(Publicacion2026, new[] { existente }).Huella;

        Assert.Equal(antes, PlanificadorImportacionFeriados.Construir(Publicacion2026, new[] { existente }).Huella);

        existente.Activo = false;

        Assert.NotEqual(antes, PlanificadorImportacionFeriados.Construir(Publicacion2026, new[] { existente }).Huella);
    }

    private static EstadoImportacionFeriado Estado(PlanImportacionFeriados plan, int mes, int dia) =>
        plan.Items.Single(x => x.Fecha == new DateOnly(2026, mes, dia)).Estado;

    private static DiaInhabil Dia(
        long id,
        DateOnly fecha,
        string descripcion,
        OrigenDiaInhabil origen,
        bool activo = true,
        bool modificado = false,
        DateOnly? fechaFuente = null
    ) => new()
    {
        DiaInhabilId = id,
        Fecha = fecha,
        Descripcion = descripcion,
        Origen = origen,
        Activo = activo,
        ModificadoManualmente = modificado,
        FechaFuente = origen == OrigenDiaInhabil.FeriadoNacional ? fechaFuente ?? fecha : null,
    };
}
