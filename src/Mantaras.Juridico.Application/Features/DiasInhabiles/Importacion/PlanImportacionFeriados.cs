using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Importacion;

public enum CategoriaImportacionFeriado
{
    FeriadoNacional = 1,
    RequiereRevision = 2,
}

public enum EstadoImportacionFeriado
{
    // No hay registro para la fecha: se crea al confirmar.
    Nuevo = 1,

    // Ya hay un día inhábil activo para la fecha: no se toca.
    Existente = 2,

    // Importado antes, sin cambios manuales, y la fuente cambió la descripción.
    Actualizable = 3,

    // Hay un registro desactivado o movido a mano: nunca se pisa.
    Conflicto = 4,
}

public sealed class ItemPlanImportacionFeriado
{
    public DateOnly Fecha { get; init; }

    public string Descripcion { get; init; } = string.Empty;

    public CategoriaImportacionFeriado Categoria { get; init; }

    public EstadoImportacionFeriado Estado { get; init; }

    public IReadOnlyList<RegistroFuenteFeriado> RegistrosFuente { get; init; } =
        Array.Empty<RegistroFuenteFeriado>();

    public DiaInhabil? Existente { get; init; }

    public string? Motivo { get; init; }
}

public sealed class OmitidoPlanImportacionFeriado
{
    public RegistroFuenteFeriado Registro { get; init; } = null!;

    public string Motivo { get; init; } = string.Empty;
}

public sealed class PlanImportacionFeriados
{
    public int Anio { get; init; }

    public IReadOnlyList<ItemPlanImportacionFeriado> Items { get; init; } =
        Array.Empty<ItemPlanImportacionFeriado>();

    public IReadOnlyList<OmitidoPlanImportacionFeriado> Omitidos { get; init; } =
        Array.Empty<OmitidoPlanImportacionFeriado>();

    // Importados de este año que la fuente ya no publica. Se informan, no se borran.
    public IReadOnlyList<DiaInhabil> AusentesEnFuente { get; init; } =
        Array.Empty<DiaInhabil>();

    // Resume la publicación y el estado de los registros afectados. Si cambia
    // cualquiera de los dos entre la vista previa y la confirmación, cambia la huella.
    public string Huella { get; init; } = string.Empty;

    public IEnumerable<ItemPlanImportacionFeriado> FeriadosNacionales =>
        Items.Where(x => x.Categoria == CategoriaImportacionFeriado.FeriadoNacional);

    public IEnumerable<ItemPlanImportacionFeriado> RequierenRevision =>
        Items.Where(x => x.Categoria == CategoriaImportacionFeriado.RequiereRevision);
}

public static class PlanificadorImportacionFeriados
{
    // Ley 27.399: solo estos tipos son feriados nacionales. Los días no laborables
    // (optativos por credo), turísticos y los de alcance regional quedan a revisión.
    private static readonly HashSet<string> TiposFeriadoNacional = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        "inamovible",
        "trasladable",
    };

    private const int LargoMaximoDescripcion = 300;

    public static bool EsFeriadoNacional(string tipo) =>
        TiposFeriadoNacional.Contains(tipo.Trim());

    public static PlanImportacionFeriados Construir(
        PublicacionFeriadosNacionales publicacion,
        IReadOnlyCollection<DiaInhabil> existentes
    )
    {
        var anio = publicacion.Anio;
        var omitidos = publicacion.Registros
            .Where(x => x.Fecha.Year != anio)
            .OrderBy(x => x.Fecha)
            .Select(x => new OmitidoPlanImportacionFeriado
            {
                Registro = x,
                Motivo = $"La fecha no pertenece a {anio}; se importa con el calendario de {x.Fecha.Year}.",
            })
            .ToArray();

        var porFecha = existentes
            .GroupBy(x => x.Fecha)
            .ToDictionary(x => x.Key, x => x.First());

        // Una misma fecha puede venir varias veces (por ejemplo, feriado y día no
        // laborable). Basta un feriado nacional para que la fecha sea nacional.
        var items = publicacion.Registros
            .Where(x => x.Fecha.Year == anio)
            .GroupBy(x => x.Fecha)
            .OrderBy(x => x.Key)
            .Select(grupo => ConstruirItem(grupo.Key, grupo.ToArray(), porFecha, existentes))
            .ToArray();

        var fechasFuente = items.Select(x => x.Fecha).ToHashSet();
        var ausentes = existentes
            .Where(x =>
                x.Origen == OrigenDiaInhabil.FeriadoNacional
                && x.FechaFuente is { } fechaFuente
                && fechaFuente.Year == anio
                && !fechasFuente.Contains(fechaFuente)
            )
            .OrderBy(x => x.FechaFuente)
            .ThenBy(x => x.DiaInhabilId)
            .ToArray();

        return new PlanImportacionFeriados
        {
            Anio = anio,
            Items = items,
            Omitidos = omitidos,
            AusentesEnFuente = ausentes,
            Huella = CalcularHuella(anio, items, ausentes),
        };
    }

    public static string NormalizarDescripcion(string texto)
    {
        var normalizado = Regex.Replace(texto.Trim(), @"\s+", " ");

        return normalizado.Length <= LargoMaximoDescripcion
            ? normalizado
            : normalizado[..LargoMaximoDescripcion].TrimEnd();
    }

    private static ItemPlanImportacionFeriado ConstruirItem(
        DateOnly fecha,
        IReadOnlyList<RegistroFuenteFeriado> registros,
        IReadOnlyDictionary<DateOnly, DiaInhabil> porFecha,
        IReadOnlyCollection<DiaInhabil> existentes
    )
    {
        var nacionales = registros.Where(x => EsFeriadoNacional(x.Tipo)).ToArray();
        var categoria = nacionales.Length > 0
            ? CategoriaImportacionFeriado.FeriadoNacional
            : CategoriaImportacionFeriado.RequiereRevision;
        var descripcion = NormalizarDescripcion(
            string.Join(
                " / ",
                (nacionales.Length > 0 ? nacionales : registros)
                    .Select(x => NormalizarDescripcion(x.Nombre))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
            )
        );

        // Primero el registro que ocupa la fecha; si no hay, un importado de esa
        // fecha que el usuario movió a otro día.
        var existente = porFecha.GetValueOrDefault(fecha)
            ?? existentes
                .Where(x =>
                    x.Origen == OrigenDiaInhabil.FeriadoNacional
                    && x.FechaFuente == fecha
                )
                .OrderBy(x => x.DiaInhabilId)
                .FirstOrDefault();

        var (estado, motivo) = Clasificar(fecha, descripcion, existente);

        if (categoria == CategoriaImportacionFeriado.RequiereRevision)
        {
            motivo = MotivoRevision(registros) + (motivo is null ? "" : " " + motivo);
        }

        return new ItemPlanImportacionFeriado
        {
            Fecha = fecha,
            Descripcion = descripcion,
            Categoria = categoria,
            Estado = estado,
            RegistrosFuente = registros,
            Existente = existente,
            Motivo = motivo,
        };
    }

    private static (EstadoImportacionFeriado Estado, string? Motivo) Clasificar(
        DateOnly fecha,
        string descripcion,
        DiaInhabil? existente
    )
    {
        if (existente is null)
        {
            return (EstadoImportacionFeriado.Nuevo, null);
        }

        if (existente.Fecha != fecha)
        {
            return (
                EstadoImportacionFeriado.Conflicto,
                $"El feriado importado para esta fecha se movió manualmente al {existente.Fecha:dd/MM/yyyy}; no se vuelve a crear."
            );
        }

        if (!existente.Activo)
        {
            return (
                EstadoImportacionFeriado.Conflicto,
                "Hay un día inhábil desactivado en esta fecha; no se reactiva automáticamente."
            );
        }

        if (
            existente.Origen == OrigenDiaInhabil.FeriadoNacional
            && !existente.ModificadoManualmente
            && existente.Descripcion != descripcion
        )
        {
            return (
                EstadoImportacionFeriado.Actualizable,
                "La fuente cambió la descripción; se actualiza porque el registro no tiene cambios manuales."
            );
        }

        if (existente.Descripcion != descripcion)
        {
            return (
                EstadoImportacionFeriado.Existente,
                existente.Origen == OrigenDiaInhabil.Manual
                    ? "Ya hay un día inhábil manual en esta fecha; se conserva su descripción."
                    : "El registro tiene cambios manuales; se conserva su descripción."
            );
        }

        return (EstadoImportacionFeriado.Existente, null);
    }

    private static string MotivoRevision(IReadOnlyList<RegistroFuenteFeriado> registros)
    {
        var tipos = registros
            .Select(x => x.Tipo.Trim().ToLowerInvariant())
            .Distinct()
            .ToArray();

        if (tipos.All(x => x == "turistico"))
        {
            return "Día no laborable con fines turísticos: no es feriado nacional. Importarlo solo si el fuero lo declara inhábil.";
        }

        if (tipos.All(x => x == "no_laborable"))
        {
            return "Día no laborable de aplicación particular (por credo o comunidad): no es feriado nacional.";
        }

        return $"Tipo {string.Join(", ", tipos.Select(x => $"'{x}'"))} no reconocido como feriado nacional (puede ser de alcance regional). Revisar antes de importar.";
    }

    private static string CalcularHuella(
        int anio,
        IReadOnlyList<ItemPlanImportacionFeriado> items,
        IReadOnlyList<DiaInhabil> ausentes
    )
    {
        var texto = new StringBuilder();
        texto.Append("v1|").Append(anio).Append('\n');

        foreach (var item in items)
        {
            texto
                .Append(item.Fecha.ToString("yyyy-MM-dd"))
                .Append('|').Append((int)item.Categoria)
                .Append('|').Append((int)item.Estado)
                .Append('|').Append(item.Descripcion);

            foreach (var registro in item.RegistrosFuente.OrderBy(x => x.Tipo).ThenBy(x => x.Nombre))
            {
                texto.Append('|').Append(registro.Tipo).Append(':').Append(registro.Nombre);
            }

            AgregarEstadoRegistro(texto, item.Existente);
            texto.Append('\n');
        }

        foreach (var ausente in ausentes)
        {
            texto.Append("ausente");
            AgregarEstadoRegistro(texto, ausente);
            texto.Append('\n');
        }

        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(texto.ToString()))
        ).ToLowerInvariant();
    }

    private static void AgregarEstadoRegistro(StringBuilder texto, DiaInhabil? registro)
    {
        if (registro is null)
        {
            texto.Append("|-");
            return;
        }

        texto
            .Append('|').Append(registro.DiaInhabilId)
            .Append('|').Append(registro.Fecha.ToString("yyyy-MM-dd"))
            .Append('|').Append(registro.Activo)
            .Append('|').Append((int)registro.Origen)
            .Append('|').Append(registro.ModificadoManualmente)
            .Append('|').Append(registro.Descripcion);
    }
}
