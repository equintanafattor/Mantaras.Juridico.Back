using System.Data.Common;
using Mantaras.Juridico.Application.Features.Expedientes.Responses;
using Mantaras.Juridico.Application.Features.Expedientes.Services;
using Mantaras.Juridico.Domain.Entities;
using Mantaras.Juridico.Domain.Enums;
using Mantaras.Juridico.Infrastructure.Persistence;
using Mantaras.Juridico.Infrastructure.Persistence.Repositories;
using Mantaras.Juridico.Tests.DiasInhabiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Mantaras.Juridico.Tests.Expedientes;

// Clientes y cliente principal en GET /api/expedientes/cobro
// (ExpedienteRepository.BuscarCobroAsync + ClientesExpedienteCobro.Resolver).
// Cada test crea sus propios datos con una marca única y los borra al terminar.
[Collection(ColeccionBaseAislada.Nombre)]
public sealed class ClientesCobroExpedientesTests : IAsyncLifetime
{
    private readonly BaseAislada _base;
    private readonly string _marca = "COBROCLI " + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    private static readonly DateOnly Hoy = new(2026, 11, 26);

    private Dictionary<string, long> _clientes = new();
    private Dictionary<string, long> _casos = new();
    private Dictionary<string, long> _expedientes = new();
    private List<long> _entradas = new();
    private bool _creado;

    public ClientesCobroExpedientesTests(BaseAislada baseAislada)
    {
        _base = baseAislada;
    }

    public async Task InitializeAsync()
    {
        if (BaseAislada.CadenaConexion is null)
        {
            return;
        }

        await using var db = _base.CrearContexto();
        var ahora = DateTime.UtcNow;
        var regla = await db.ReglasVencimiento.SingleAsync(x =>
            x.Codigo == ReglaVencimiento.CodigoSeguimientoCobroJudicial
        );

        Cliente NuevoCliente(string apellido, bool activo = true) => new()
        {
            Nombre = "Ana", Apellido = $"{_marca} {apellido}", Dni = $"T{_marca[^8..]}{apellido}",
            Activo = activo, FechaCreacion = ahora, UsuarioCreacion = "test",
        };

        var cA = NuevoCliente("A");
        var cB = NuevoCliente("B");
        var cC = NuevoCliente("C", activo: false);
        var cD = NuevoCliente("D");

        Caso NuevoCaso(string clave, params (Cliente Cliente, bool Principal)[] clientes)
        {
            var caso = new Caso
            {
                Titulo = $"{_marca} caso {clave}", FaseInterna = "PRUEBA", FechaCreacion = ahora, Activo = true,
            };

            foreach (var (cliente, principal) in clientes)
            {
                caso.Clientes.Add(new CasoCliente
                {
                    Caso = caso, Cliente = cliente, EsPrincipal = principal,
                    TipoParticipacion = principal ? TipoParticipacionCliente.Titular : TipoParticipacionCliente.Otro,
                });
            }

            return caso;
        }

        var casos = new Dictionary<string, Caso>
        {
            ["A+B"] = NuevoCaso("A+B", (cA, true), (cB, false)),
            ["C"] = NuevoCaso("C", (cC, true)),
            ["A"] = NuevoCaso("A", (cA, true)),
            ["vacio"] = NuevoCaso("vacio"),
            // Dato histórico: un único cliente sin marca de principal.
            ["D sin marca"] = NuevoCaso("D sin marca", (cD, false)),
        };

        db.AddRange(cA, cB, cC, cD);
        db.AddRange(casos.Values);

        // (clave, casos). Vencimientos crecientes: el orden del listado es determinista.
        var definiciones = new (string Clave, string[] Casos)[]
        {
            ("e1 un caso con principal y secundario", new[] { "A+B" }),
            ("e2 ambiguo", new[] { "A+B", "C" }),
            ("e3 mismo principal en dos casos", new[] { "A+B", "A" }),
            ("e4 caso sin clientes", new[] { "vacio" }),
            ("e5 sin casos", Array.Empty<string>()),
            ("e6 unico cliente sin marca", new[] { "D sin marca" }),
            ("e7 principal inactivo", new[] { "C" }),
        };

        var expedientes = new Dictionary<string, Expediente>();
        var generaciones = new List<AgendaGeneracionRegla>();
        var dia = 0;

        foreach (var (clave, clavesCasos) in definiciones)
        {
            var expediente = new Expediente
            {
                Caratula = $"{_marca} {clave}", TipoExpediente = TipoExpediente.Principal,
                Activo = true, FechaCreacion = ahora, UsuarioCreacion = "test",
            };

            foreach (var claveCaso in clavesCasos)
            {
                expediente.Casos.Add(new CasoExpediente { Caso = casos[claveCaso], Expediente = expediente });
            }

            var entrada = new EntradaAgenda
            {
                TipoEntradaAgendaId = regla.TipoEntradaAgendaId, Titulo = $"{_marca} seguimiento {clave}",
                FechaInicio = Hoy, FechaVencimiento = Hoy.AddDays(++dia),
                FechaCreacion = ahora, UsuarioCreacion = "test",
            };
            var generacion = new AgendaGeneracionRegla
            {
                ReglaVencimientoId = regla.ReglaVencimientoId, EntradaAgenda = entrada, FechaBase = Hoy,
                OrigenFechaBase = OrigenFechaBaseAgenda.ExpedienteJudicial, ExpedienteOrigen = expediente,
                ClaveIdempotencia = $"{_marca} {clave}", FechaCreacion = ahora, UsuarioCreacion = "test",
            };
            expediente.EnviosCobro.Add(new EnvioCobroExpediente
            {
                TipoDocumento = TipoDocumentoCobro.Cedula, FechaEnvio = Hoy, AgendaGeneracionRegla = generacion,
                FechaCreacion = ahora, UsuarioCreacion = "test",
            });

            expedientes[clave[..2]] = expediente;
            generaciones.Add(generacion);
            db.Add(expediente);
        }

        await db.SaveChangesAsync();
        _creado = true;

        _clientes = new() { ["A"] = cA.ClienteId, ["B"] = cB.ClienteId, ["C"] = cC.ClienteId, ["D"] = cD.ClienteId };
        _casos = casos.ToDictionary(x => x.Key, x => x.Value.CasoId);
        _expedientes = expedientes.ToDictionary(x => x.Key, x => x.Value.ExpedienteId);
        _entradas = generaciones.Select(x => x.EntradaAgendaId).ToList();
    }

    public async Task DisposeAsync()
    {
        if (BaseAislada.CadenaConexion is null || !_creado)
        {
            return;
        }

        await using var db = _base.CrearContexto();
        var expedientes = _expedientes.Values.ToArray();
        var casos = _casos.Values.ToArray();
        var clientes = _clientes.Values.ToArray();

        await db.EnviosCobroExpedientes.Where(x => expedientes.Contains(x.ExpedienteId)).ExecuteDeleteAsync();
        await db.AgendaGeneracionesReglas.Where(x => _entradas.Contains(x.EntradaAgendaId)).ExecuteDeleteAsync();
        await db.EntradasAgenda.Where(x => _entradas.Contains(x.EntradaAgendaId)).ExecuteDeleteAsync();
        await db.CasosExpedientes.Where(x => expedientes.Contains(x.ExpedienteId)).ExecuteDeleteAsync();
        await db.Expedientes.Where(x => expedientes.Contains(x.ExpedienteId)).ExecuteDeleteAsync();
        await db.CasosClientes.Where(x => casos.Contains(x.CasoId)).ExecuteDeleteAsync();
        await db.Casos.Where(x => casos.Contains(x.CasoId)).ExecuteDeleteAsync();
        await db.Clientes.Where(x => clientes.Contains(x.ClienteId)).ExecuteDeleteAsync();
    }

    private sealed record Fila(
        long ExpedienteId,
        IReadOnlyCollection<ClienteExpedienteCobroResponse> Clientes,
        long? ClientePrincipalId
    );

    private async Task<(Fila[] Filas, int Total, int Consultas)> Buscar(int page = 1, int pageSize = 20)
    {
        var contador = new ContadorConsultas();
        await using var db = new JuridicoDbContext(
            new DbContextOptionsBuilder<JuridicoDbContext>()
                .UseNpgsql(BaseAislada.CadenaConexion!)
                .AddInterceptors(contador)
                .Options
        );
        var repo = new ExpedienteRepository(db);
        var expedientes = await repo.BuscarCobroAsync(_marca, true, null, Hoy, page, pageSize);
        var consultasListado = contador.Cantidad;
        var total = await repo.ContarCobroAsync(_marca, true, null, Hoy);

        var filas = expedientes
            .Select(expediente =>
            {
                var (clientes, principal) = ClientesExpedienteCobro.Resolver(expediente);
                return new Fila(expediente.ExpedienteId, clientes, principal);
            })
            .ToArray();

        return (filas, total, consultasListado);
    }

    private async Task<Fila> FilaDe(string expediente)
    {
        var (filas, _, _) = await Buscar();
        return filas.Single(x => x.ExpedienteId == _expedientes[expediente]);
    }

    private long[] Clientes(params string[] claves) => claves.Select(x => _clientes[x]).ToArray();

    [FactConBase]
    public async Task Principal_y_secundario_en_un_caso_devuelve_el_principal()
    {
        var fila = await FilaDe("e1");

        Assert.Equal(_clientes["A"], fila.ClientePrincipalId);
        Assert.Equal(Clientes("A", "B"), fila.Clientes.Select(x => x.ClienteId));
        var a = fila.Clientes.First();
        Assert.True(a.EsPrincipal);
        Assert.Equal($"{_marca} A, Ana", a.NombreCompleto);
        Assert.Equal("Ana", a.Nombre);
        Assert.Equal($"T{_marca[^8..]}A", a.Dni);
        Assert.False(fila.Clientes.Last().EsPrincipal);
    }

    [FactConBase]
    public async Task Principales_distintos_en_varios_casos_es_ambiguo()
    {
        var fila = await FilaDe("e2");

        Assert.Null(fila.ClientePrincipalId);
        // Principales primero (A, C por apellido), después el secundario B.
        Assert.Equal(Clientes("A", "C", "B"), fila.Clientes.Select(x => x.ClienteId));
        Assert.Equal(new[] { true, true, false }, fila.Clientes.Select(x => x.EsPrincipal));
    }

    [FactConBase]
    public async Task Mismo_principal_en_varios_casos_no_se_duplica()
    {
        var fila = await FilaDe("e3");

        Assert.Equal(_clientes["A"], fila.ClientePrincipalId);
        Assert.Equal(Clientes("A", "B"), fila.Clientes.Select(x => x.ClienteId));
    }

    [FactConBase]
    public async Task Sin_clientes_o_sin_casos_devuelve_lista_vacia()
    {
        var sinClientes = await FilaDe("e4");
        Assert.Null(sinClientes.ClientePrincipalId);
        Assert.Empty(sinClientes.Clientes);

        var sinCasos = await FilaDe("e5");
        Assert.Null(sinCasos.ClientePrincipalId);
        Assert.Empty(sinCasos.Clientes);
    }

    [FactConBase]
    public async Task Unico_cliente_sin_marca_es_el_principal()
    {
        var fila = await FilaDe("e6");

        Assert.Equal(_clientes["D"], fila.ClientePrincipalId);
        Assert.False(Assert.Single(fila.Clientes).EsPrincipal);
    }

    [FactConBase]
    public async Task Cliente_inactivo_se_devuelve_con_su_estado()
    {
        var fila = await FilaDe("e7");

        Assert.Equal(_clientes["C"], fila.ClientePrincipalId);
        Assert.False(Assert.Single(fila.Clientes).Activo);
    }

    [FactConBase]
    public async Task Cliente_con_varios_judiciales_aparece_en_cada_fila()
    {
        var (filas, _, _) = await Buscar();
        var conA = filas.Where(x => x.Clientes.Any(c => c.ClienteId == _clientes["A"])).ToArray();

        Assert.Equal(3, conA.Length);
        Assert.Equal(new[] { _expedientes["e1"], _expedientes["e2"], _expedientes["e3"] }, conA.Select(x => x.ExpedienteId));
    }

    [FactConBase]
    public async Task Una_fila_por_judicial_con_paginacion_y_total_sin_cambios()
    {
        var p1 = await Buscar(page: 1, pageSize: 3);
        var p2 = await Buscar(page: 2, pageSize: 3);
        var p3 = await Buscar(page: 3, pageSize: 3);

        Assert.All(new[] { p1, p2, p3 }, x => Assert.Equal(7, x.Total));
        Assert.Equal(new[] { 3, 3, 1 }, new[] { p1.Filas.Length, p2.Filas.Length, p3.Filas.Length });

        var ids = p1.Filas.Concat(p2.Filas).Concat(p3.Filas).Select(x => x.ExpedienteId).ToArray();
        // Orden por vencimiento: e1..e7, sin repetidos aunque e2 y e3 tengan varios casos y clientes.
        Assert.Equal(new[] { "e1", "e2", "e3", "e4", "e5", "e6", "e7" }.Select(x => _expedientes[x]), ids);
    }

    [FactConBase]
    public async Task Los_clientes_se_cargan_en_una_sola_consulta()
    {
        var (filas, _, consultas) = await Buscar();

        Assert.Equal(7, filas.Length);
        Assert.Equal(1, consultas);
    }

    private sealed class ContadorConsultas : DbCommandInterceptor
    {
        public int Cantidad { get; private set; }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default
        )
        {
            Cantidad++;
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}
