using System.Globalization;
using System.Net;
using System.Text.Json;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.DiasInhabiles;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Importacion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mantaras.Juridico.Infrastructure.Services;

public sealed class FeriadosNacionalesOptions
{
    public const string SectionName = "FeriadosNacionales";

    // Recurso anual del dataset jgm-feriados-nacionales. Que exista un año no
    // garantiza que exista el siguiente: un 404 se informa como AnioNoDisponible.
    public string UrlPlantilla { get; set; } =
        "https://www.argentina.gob.ar/sites/default/files/holidays-{anio}-es.json";

    public int TimeoutSegundos { get; set; } = 15;
}

// Lee el calendario anual publicado en argentina.gob.ar (schema.org Dataset con
// mainEntity.itemListElement[].item = Event). La lectura es estricta: ante
// cualquier registro ilegible se descarta toda la publicación.
public sealed class FuenteFeriadosArgentinaGobAr : IFuenteFeriadosNacionales
{
    private const long TamanioMaximoBytes = 2 * 1024 * 1024;

    private readonly HttpClient _httpClient;
    private readonly FeriadosNacionalesOptions _options;
    private readonly ILogger<FuenteFeriadosArgentinaGobAr> _logger;

    public FuenteFeriadosArgentinaGobAr(
        HttpClient httpClient,
        IOptions<FeriadosNacionalesOptions> options,
        ILogger<FuenteFeriadosArgentinaGobAr> logger
    )
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    internal static void Configurar(HttpClient httpClient, FeriadosNacionalesOptions options)
    {
        httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.TimeoutSegundos));
        httpClient.MaxResponseContentBufferSize = TamanioMaximoBytes;
    }

    public string Codigo => "argentina.gob.ar";

    public string Nombre => "Feriados nacionales - Ministerio del Interior (argentina.gob.ar)";

    public string ConstruirUrl(int anio) =>
        _options.UrlPlantilla.Replace(
            "{anio}",
            anio.ToString(CultureInfo.InvariantCulture),
            StringComparison.Ordinal
        );

    public async Task<Result<PublicacionFeriadosNacionales>> ObtenerAsync(
        int anio,
        CancellationToken cancellationToken = default
    )
    {
        var url = ConstruirUrl(anio);
        byte[] contenido;

        try
        {
            using var respuesta = await _httpClient.GetAsync(url, cancellationToken);

            if (respuesta.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
            {
                return Result<PublicacionFeriadosNacionales>.Failure(
                    DiaInhabilErrors.AnioNoDisponible
                );
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "La fuente de feriados respondió {Estado} para {Url}.",
                    (int)respuesta.StatusCode,
                    url
                );
                return Result<PublicacionFeriadosNacionales>.Failure(
                    DiaInhabilErrors.FuenteNoDisponible
                );
            }

            var tipoContenido = respuesta.Content.Headers.ContentType?.MediaType;

            if (tipoContenido is null || !tipoContenido.Contains("json", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning(
                    "La fuente de feriados devolvió {TipoContenido} en lugar de JSON para {Url}.",
                    tipoContenido,
                    url
                );
                return Result<PublicacionFeriadosNacionales>.Failure(
                    DiaInhabilErrors.FuenteInvalida
                );
            }

            contenido = await respuesta.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("La fuente de feriados no respondió a tiempo: {Url}.", url);
            return Result<PublicacionFeriadosNacionales>.Failure(
                DiaInhabilErrors.FuenteNoDisponible
            );
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "No se pudo consultar la fuente de feriados: {Url}.", url);
            return Result<PublicacionFeriadosNacionales>.Failure(
                DiaInhabilErrors.FuenteNoDisponible
            );
        }

        var publicacion = Leer(anio, url, contenido);

        if (publicacion is null)
        {
            _logger.LogWarning("La fuente de feriados devolvió datos inválidos: {Url}.", url);
            return Result<PublicacionFeriadosNacionales>.Failure(DiaInhabilErrors.FuenteInvalida);
        }

        return Result<PublicacionFeriadosNacionales>.Success(publicacion);
    }

    internal static PublicacionFeriadosNacionales? Leer(int anio, string url, byte[] contenido)
    {
        try
        {
            using var documento = JsonDocument.Parse(contenido);
            var raiz = documento.RootElement;

            if (raiz.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (
                raiz.TryGetProperty("temporalCoverage", out var cobertura)
                && cobertura.ValueKind == JsonValueKind.String
                && cobertura.GetString()?.Trim() != anio.ToString(CultureInfo.InvariantCulture)
            )
            {
                return null;
            }

            if (
                !raiz.TryGetProperty("mainEntity", out var entidad)
                || entidad.ValueKind != JsonValueKind.Object
                || !entidad.TryGetProperty("itemListElement", out var elementos)
                || elementos.ValueKind != JsonValueKind.Array
            )
            {
                return null;
            }

            var registros = new List<RegistroFuenteFeriado>();

            foreach (var elemento in elementos.EnumerateArray())
            {
                var registro = LeerRegistro(elemento);

                if (registro is null)
                {
                    return null;
                }

                registros.Add(registro);
            }

            if (
                entidad.TryGetProperty("numberOfItems", out var cantidad)
                && cantidad.ValueKind == JsonValueKind.Number
                && cantidad.GetInt32() != registros.Count
            )
            {
                return null;
            }

            // Un año sin ningún feriado nacional propio indica un archivo equivocado.
            if (!registros.Any(x =>
                x.Fecha.Year == anio && PlanificadorImportacionFeriados.EsFeriadoNacional(x.Tipo)
            ))
            {
                return null;
            }

            DateTime? fechaPublicacion =
                raiz.TryGetProperty("dateModified", out var modificado)
                && modificado.ValueKind == JsonValueKind.String
                && DateTimeOffset.TryParse(
                    modificado.GetString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var fechaModificado
                )
                    ? fechaModificado.UtcDateTime
                    : null;

            return new PublicacionFeriadosNacionales(anio, url, fechaPublicacion, registros);
        }
        catch (Exception exception) when (
            exception is JsonException or InvalidOperationException or FormatException
        )
        {
            return null;
        }
    }

    private static RegistroFuenteFeriado? LeerRegistro(JsonElement elemento)
    {
        if (
            elemento.ValueKind != JsonValueKind.Object
            || !elemento.TryGetProperty("item", out var item)
            || item.ValueKind != JsonValueKind.Object
        )
        {
            return null;
        }

        var inicio = LeerTexto(item, "startDate");
        var nombre = LeerTexto(item, "name");
        var tipo = LeerTipo(item);

        if (
            inicio is null
            || string.IsNullOrWhiteSpace(nombre)
            || string.IsNullOrWhiteSpace(tipo)
            || !DateOnly.TryParseExact(
                inicio,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fecha
            )
        )
        {
            return null;
        }

        // Hoy cada evento es de un solo día. Un rango sería un formato nuevo que
        // conviene revisar antes de importar.
        var fin = LeerTexto(item, "endDate");

        if (fin is not null && fin != inicio)
        {
            return null;
        }

        return new RegistroFuenteFeriado(fecha, nombre.Trim(), tipo.Trim());
    }

    private static string? LeerTipo(JsonElement item)
    {
        if (!item.TryGetProperty("additionalProperty", out var propiedades))
        {
            return null;
        }

        var candidatas = propiedades.ValueKind switch
        {
            JsonValueKind.Object => new[] { propiedades },
            JsonValueKind.Array => propiedades.EnumerateArray().ToArray(),
            _ => Array.Empty<JsonElement>(),
        };

        return candidatas
            .Where(x => x.ValueKind == JsonValueKind.Object && LeerTexto(x, "name") == "tipo")
            .Select(x => LeerTexto(x, "value"))
            .FirstOrDefault();
    }

    private static string? LeerTexto(JsonElement elemento, string propiedad) =>
        elemento.TryGetProperty(propiedad, out var valor) && valor.ValueKind == JsonValueKind.String
            ? valor.GetString()
            : null;
}
