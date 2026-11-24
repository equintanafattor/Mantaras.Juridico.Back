using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Mantaras.Juridico.Application.Common.Exceptions;
using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Common.Results;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Importacion;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Requests;
using Mantaras.Juridico.Application.Features.DiasInhabiles.Responses;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;

namespace Mantaras.Juridico.Application.Features.DiasInhabiles.Services;

public sealed class ImportacionFeriadosService : IImportacionFeriadosService
{
    public const int AnioMinimo = 2000;
    public const int AnioMaximo = 2100;

    private readonly IDiaInhabilRepository _repository;
    private readonly IFuenteFeriadosNacionales _fuente;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<ConfirmarImportacionFeriadosRequest> _confirmarValidator;

    public ImportacionFeriadosService(
        IDiaInhabilRepository repository,
        IFuenteFeriadosNacionales fuente,
        ICurrentUserService currentUser,
        IValidator<ConfirmarImportacionFeriadosRequest> confirmarValidator
    )
    {
        _repository = repository;
        _fuente = fuente;
        _currentUser = currentUser;
        _confirmarValidator = confirmarValidator;
    }

    public async Task<Result<VistaPreviaImportacionFeriadosResponse>> ObtenerVistaPreviaAsync(
        int anio,
        CancellationToken cancellationToken = default
    )
    {
        if (ValidarAnio(anio) is { } errorAnio)
        {
            return Result<VistaPreviaImportacionFeriadosResponse>.Failure(errorAnio);
        }

        var publicacion = await _fuente.ObtenerAsync(anio, cancellationToken);

        if (publicacion.IsFailure)
        {
            return Result<VistaPreviaImportacionFeriadosResponse>.Failure(publicacion.Errors);
        }

        var existentes = await _repository.ObtenerParaImportacionAsync(anio, cancellationToken);
        var calendario = await _repository.ObtenerCalendarioAsync(
            anio,
            seguimiento: false,
            cancellationToken
        );
        var plan = PlanificadorImportacionFeriados.Construir(publicacion.Value!, existentes);
        var nacionales = plan.FeriadosNacionales.ToArray();

        return Result<VistaPreviaImportacionFeriadosResponse>.Success(
            new VistaPreviaImportacionFeriadosResponse
            {
                Anio = anio,
                Fuente = MapearFuente(publicacion.Value!.Url),
                FechaPublicacionFuente = publicacion.Value.FechaPublicacion,
                ConsultadoEn = DateTime.UtcNow,
                Huella = plan.Huella,
                CalendarioCargado = calendario is not null,
                UltimaSincronizacion = calendario?.FechaSincronizacion,
                Resumen = new ResumenImportacionFeriadosResponse
                {
                    Nuevos = Contar(nacionales, EstadoImportacionFeriado.Nuevo),
                    Existentes = Contar(nacionales, EstadoImportacionFeriado.Existente),
                    Actualizables = Contar(nacionales, EstadoImportacionFeriado.Actualizable),
                    Conflictos = Contar(nacionales, EstadoImportacionFeriado.Conflicto),
                    RequierenRevision = plan.RequierenRevision.Count(),
                    Omitidos = plan.Omitidos.Count,
                    AusentesEnFuente = plan.AusentesEnFuente.Count,
                },
                Items = nacionales.Select(MapearItem).ToArray(),
                RequierenRevision = plan.RequierenRevision.Select(MapearItem).ToArray(),
                Omitidos = plan.Omitidos
                    .Select(x => new OmitidoImportacionFeriadoResponse
                    {
                        Fecha = x.Registro.Fecha,
                        Descripcion = x.Registro.Nombre,
                        Tipo = x.Registro.Tipo,
                        Motivo = x.Motivo,
                    })
                    .ToArray(),
                AusentesEnFuente = plan.AusentesEnFuente
                    .Select(DiasInhabilesService.Mapear)
                    .ToArray(),
            }
        );
    }

    public async Task<Result<ConfirmacionImportacionFeriadosResponse>> ConfirmarAsync(
        int anio,
        ConfirmarImportacionFeriadosRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (ValidarAnio(anio) is { } errorAnio)
        {
            return Result<ConfirmacionImportacionFeriadosResponse>.Failure(errorAnio);
        }

        var validacion = await _confirmarValidator.ValidateAsync(request, cancellationToken);

        if (!validacion.IsValid)
        {
            return Result<ConfirmacionImportacionFeriadosResponse>.Failure(
                DiaInhabilErrors.DatosInvalidos(
                    string.Join(" ", validacion.Errors.Select(x => x.ErrorMessage).Distinct())
                )
            );
        }

        try
        {
            return await ConfirmarUnaVezAsync(anio, request, cancellationToken);
        }
        catch (RegistroConcurrenteException)
        {
            // Otra confirmación guardó primero. Al reintentar con datos frescos,
            // la misma huella devuelve el resultado ya aplicado y otra distinta
            // queda desactualizada.
            _repository.DescartarCambios();
            return await ConfirmarUnaVezAsync(anio, request, cancellationToken);
        }
    }

    public async Task<Result<EstadoCalendarioFeriadosResponse>> ObtenerEstadoAsync(
        int anio,
        CancellationToken cancellationToken = default
    )
    {
        if (ValidarAnio(anio) is { } errorAnio)
        {
            return Result<EstadoCalendarioFeriadosResponse>.Failure(errorAnio);
        }

        var calendario = await _repository.ObtenerCalendarioAsync(
            anio,
            seguimiento: false,
            cancellationToken
        );
        var (total, importados, manuales) = await _repository.ContarActivosPorOrigenAsync(
            anio,
            cancellationToken
        );

        return Result<EstadoCalendarioFeriadosResponse>.Success(
            new EstadoCalendarioFeriadosResponse
            {
                Anio = anio,
                CalendarioCargado = calendario is not null,
                Fuente = calendario is null ? null : MapearFuente(calendario.UrlFuente),
                FechaPublicacionFuente = calendario?.FechaPublicacionFuente,
                PrimeraSincronizacion = calendario?.FechaPrimeraSincronizacion,
                UltimaSincronizacion = calendario?.FechaSincronizacion,
                UsuarioSincronizacion = calendario?.UsuarioSincronizacion,
                DiasInhabilesActivos = total,
                FeriadosImportadosActivos = importados,
                DiasManualesActivos = manuales,
            }
        );
    }

    private async Task<Result<ConfirmacionImportacionFeriadosResponse>> ConfirmarUnaVezAsync(
        int anio,
        ConfirmarImportacionFeriadosRequest request,
        CancellationToken cancellationToken
    )
    {
        var fechasRevision = request.FechasRevisionIncluidas.Distinct().Order().ToArray();
        var claveConfirmacion = ConstruirClaveConfirmacion(request.Huella, fechasRevision);
        var yaAplicada = await ObtenerConfirmacionAplicadaAsync(
            anio,
            claveConfirmacion,
            cancellationToken
        );

        if (yaAplicada is not null)
        {
            return Result<ConfirmacionImportacionFeriadosResponse>.Success(yaAplicada);
        }

        // La descarga queda fuera del bloqueo para no retenerlo mientras responde la fuente.
        var publicacion = await _fuente.ObtenerAsync(anio, cancellationToken);

        if (publicacion.IsFailure)
        {
            return Result<ConfirmacionImportacionFeriadosResponse>.Failure(publicacion.Errors);
        }

        await using var transaccion = await _repository.IniciarImportacionExclusivaAsync(
            anio,
            cancellationToken
        );

        // Otra confirmación pudo aplicarse mientras se descargaba o se esperaba el bloqueo.
        var calendario = await _repository.ObtenerCalendarioAsync(
            anio,
            seguimiento: true,
            cancellationToken
        );

        if (calendario?.UltimaClaveConfirmacion == claveConfirmacion)
        {
            return Result<ConfirmacionImportacionFeriadosResponse>.Success(
                MapearConfirmacion(calendario, yaAplicada: true)
            );
        }

        var existentes = await _repository.ObtenerParaImportacionAsync(anio, cancellationToken);
        var plan = PlanificadorImportacionFeriados.Construir(publicacion.Value!, existentes);

        if (plan.Huella != request.Huella)
        {
            return Result<ConfirmacionImportacionFeriadosResponse>.Failure(
                DiaInhabilErrors.VistaPreviaDesactualizada
            );
        }

        var revisionPorFecha = plan.RequierenRevision.ToDictionary(x => x.Fecha);
        var fechasNoRevisables = fechasRevision
            .Where(x => !revisionPorFecha.ContainsKey(x))
            .ToArray();

        if (fechasNoRevisables.Length > 0)
        {
            return Result<ConfirmacionImportacionFeriadosResponse>.Failure(
                DiaInhabilErrors.DatosInvalidos(
                    "Solo se pueden incluir fechas listadas en requierenRevision: "
                    + string.Join(", ", fechasNoRevisables.Select(x => x.ToString("yyyy-MM-dd")))
                    + "."
                )
            );
        }

        var ahora = DateTime.UtcNow;
        var aplicar = plan.FeriadosNacionales
            .Concat(fechasRevision.Select(x => revisionPorFecha[x]))
            .ToArray();
        var creados = 0;
        var actualizados = 0;

        foreach (var item in aplicar)
        {
            if (item.Estado == EstadoImportacionFeriado.Nuevo)
            {
                await _repository.AgregarAsync(
                    new DiaInhabil
                    {
                        Fecha = item.Fecha,
                        Descripcion = item.Descripcion,
                        Origen = OrigenDiaInhabil.FeriadoNacional,
                        Fuente = _fuente.Codigo,
                        FechaFuente = item.Fecha,
                        FechaSincronizacion = ahora,
                        Activo = true,
                        FechaCreacion = ahora,
                        UsuarioCreacion = _currentUser.Usuario,
                    },
                    cancellationToken
                );
                creados++;
            }
            else if (item.Estado == EstadoImportacionFeriado.Actualizable)
            {
                var existente = item.Existente!;
                existente.Descripcion = item.Descripcion;
                existente.FechaSincronizacion = ahora;
                existente.FechaModificacion = ahora;
                existente.UsuarioModificacion = _currentUser.Usuario;
                actualizados++;
            }
        }

        if (calendario is null)
        {
            calendario = new CalendarioFeriadosImportado
            {
                Anio = anio,
                FechaPrimeraSincronizacion = ahora,
            };
            await _repository.AgregarCalendarioAsync(calendario, cancellationToken);
        }

        calendario.Fuente = _fuente.Codigo;
        calendario.UrlFuente = publicacion.Value!.Url;
        calendario.FechaPublicacionFuente = publicacion.Value.FechaPublicacion;
        calendario.FechaSincronizacion = ahora;
        calendario.UsuarioSincronizacion = _currentUser.Usuario;
        calendario.UltimaClaveConfirmacion = claveConfirmacion;
        calendario.Creados = creados;
        calendario.Actualizados = actualizados;
        calendario.Existentes = Contar(aplicar, EstadoImportacionFeriado.Existente);
        calendario.Conflictos = Contar(aplicar, EstadoImportacionFeriado.Conflicto);
        calendario.RevisionIncluidos = fechasRevision.Length;

        // Todo en un único SaveChanges: si falla, no queda una importación a medias.
        await _repository.GuardarImportacionAsync(cancellationToken);
        await transaccion.ConfirmarAsync(cancellationToken);

        return Result<ConfirmacionImportacionFeriadosResponse>.Success(
            MapearConfirmacion(calendario, yaAplicada: false)
        );
    }

    private async Task<ConfirmacionImportacionFeriadosResponse?> ObtenerConfirmacionAplicadaAsync(
        int anio,
        string claveConfirmacion,
        CancellationToken cancellationToken
    )
    {
        var calendario = await _repository.ObtenerCalendarioAsync(
            anio,
            seguimiento: false,
            cancellationToken
        );

        return calendario?.UltimaClaveConfirmacion == claveConfirmacion
            ? MapearConfirmacion(calendario, yaAplicada: true)
            : null;
    }

    private static Error? ValidarAnio(int anio) =>
        anio is < AnioMinimo or > AnioMaximo
            ? DiaInhabilErrors.DatosInvalidos(
                $"El año debe estar entre {AnioMinimo} y {AnioMaximo}."
            )
            : null;

    private static string ConstruirClaveConfirmacion(string huella, IEnumerable<DateOnly> fechas)
    {
        var texto = huella + "|" + string.Join(",", fechas.Select(x => x.ToString("yyyy-MM-dd")));

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(texto)))
            .ToLowerInvariant();
    }

    private static int Contar(
        IEnumerable<ItemPlanImportacionFeriado> items,
        EstadoImportacionFeriado estado
    ) => items.Count(x => x.Estado == estado);

    private FuenteFeriadosResponse MapearFuente(string url) => new()
    {
        Codigo = _fuente.Codigo,
        Nombre = _fuente.Nombre,
        Url = url,
    };

    private ConfirmacionImportacionFeriadosResponse MapearConfirmacion(
        CalendarioFeriadosImportado calendario,
        bool yaAplicada
    ) => new()
    {
        Anio = calendario.Anio,
        Fuente = MapearFuente(calendario.UrlFuente),
        Creados = calendario.Creados,
        Actualizados = calendario.Actualizados,
        Existentes = calendario.Existentes,
        Conflictos = calendario.Conflictos,
        RevisionIncluidos = calendario.RevisionIncluidos,
        SincronizadoEn = calendario.FechaSincronizacion,
        YaAplicada = yaAplicada,
    };

    private static ItemImportacionFeriadoResponse MapearItem(ItemPlanImportacionFeriado item) => new()
    {
        Fecha = item.Fecha,
        Descripcion = item.Descripcion,
        Categoria = item.Categoria,
        Estado = item.Estado,
        RegistrosFuente = item.RegistrosFuente
            .Select(x => new RegistroFuenteFeriadoResponse { Nombre = x.Nombre, Tipo = x.Tipo })
            .ToArray(),
        DiaInhabilIdExistente = item.Existente?.DiaInhabilId,
        FechaExistente = item.Existente?.Fecha,
        OrigenExistente = item.Existente?.Origen,
        DescripcionExistente = item.Existente?.Descripcion,
        ActivoExistente = item.Existente?.Activo,
        ModificadoManualmenteExistente = item.Existente?.ModificadoManualmente,
        Motivo = item.Motivo,
    };
}
