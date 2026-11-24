using System.Net;
using System.Text;
using Mantaras.Juridico.Application.Features.DiasInhabiles;
using Mantaras.Juridico.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Mantaras.Juridico.Tests.DiasInhabiles;

public class FuenteFeriadosArgentinaGobArTests
{
    [Fact]
    public void Lee_el_recurso_oficial_2026_completo()
    {
        var publicacion = FuenteFeriadosArgentinaGobAr.Leer(2026, "url", Fixtures.Leer(2026));

        Assert.NotNull(publicacion);
        Assert.Equal(35, publicacion!.Registros.Count);
        Assert.Equal(new DateTime(2026, 10, 2, 12, 42, 31, DateTimeKind.Utc), publicacion.FechaPublicacion);
        Assert.Contains(publicacion.Registros, x => x.Tipo == "especial" && x.Fecha == new DateOnly(2026, 11, 10));
        Assert.Contains(publicacion.Registros, x => x.Fecha == new DateOnly(2027, 1, 1));
        Assert.Equal("Año Nuevo", publicacion.Registros[0].Nombre);
    }

    [Fact]
    public void Lee_el_recurso_oficial_2025()
    {
        var publicacion = FuenteFeriadosArgentinaGobAr.Leer(2025, "url", Fixtures.Leer(2025));

        Assert.NotNull(publicacion);
        Assert.Equal(32, publicacion!.Registros.Count);
    }

    [Fact]
    public void Rechaza_un_archivo_de_otro_anio()
    {
        Assert.Null(FuenteFeriadosArgentinaGobAr.Leer(2027, "url", Fixtures.Leer(2026)));
    }

    [Theory]
    [InlineData("no es json")]
    [InlineData("[]")]
    [InlineData("{\"mainEntity\":{}}")]
    // Registro sin tipo: se descarta toda la publicación, no solo ese registro.
    [InlineData("{\"mainEntity\":{\"itemListElement\":[{\"item\":{\"name\":\"Navidad\",\"startDate\":\"2026-12-25\"}}]}}")]
    [InlineData("{\"mainEntity\":{\"itemListElement\":[{\"item\":{\"name\":\"Navidad\",\"startDate\":\"25/12/2026\",\"additionalProperty\":{\"name\":\"tipo\",\"value\":\"inamovible\"}}}]}}")]
    // Solo días no laborables: no parece un calendario nacional válido.
    [InlineData("{\"mainEntity\":{\"itemListElement\":[{\"item\":{\"name\":\"X\",\"startDate\":\"2026-04-24\",\"additionalProperty\":{\"name\":\"tipo\",\"value\":\"no_laborable\"}}}]}}")]
    // numberOfItems no coincide: respuesta truncada.
    [InlineData("{\"mainEntity\":{\"numberOfItems\":2,\"itemListElement\":[{\"item\":{\"name\":\"Navidad\",\"startDate\":\"2026-12-25\",\"additionalProperty\":{\"name\":\"tipo\",\"value\":\"inamovible\"}}}]}}")]
    public void Rechaza_datos_invalidos(string json)
    {
        Assert.Null(FuenteFeriadosArgentinaGobAr.Leer(2026, "url", Encoding.UTF8.GetBytes(json)));
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, "application/json", "DiasInhabiles.AnioNoDisponible")]
    [InlineData(HttpStatusCode.InternalServerError, "application/json", "DiasInhabiles.FuenteNoDisponible")]
    [InlineData(HttpStatusCode.OK, "text/html", "DiasInhabiles.FuenteInvalida")]
    public async Task Traduce_respuestas_http(HttpStatusCode estado, string tipo, string codigo)
    {
        var fuente = CrearFuente(_ => new HttpResponseMessage(estado)
        {
            Content = new StringContent("<html></html>", Encoding.UTF8, tipo),
        });

        var result = await fuente.ObtenerAsync(2026);

        Assert.True(result.IsFailure);
        Assert.Equal(codigo, result.Errors.Single().Code);
    }

    [Fact]
    public async Task Sin_conexion_informa_fuente_no_disponible()
    {
        var fuente = CrearFuente(_ => throw new HttpRequestException("sin red"));

        var result = await fuente.ObtenerAsync(2026);

        Assert.Equal(DiaInhabilErrors.FuenteNoDisponible, result.Errors.Single());
    }

    [Fact]
    public async Task Timeout_informa_fuente_no_disponible()
    {
        var fuente = CrearFuente(_ => throw new TaskCanceledException("timeout"));

        var result = await fuente.ObtenerAsync(2026);

        Assert.Equal(DiaInhabilErrors.FuenteNoDisponible, result.Errors.Single());
    }

    [Fact]
    public async Task Construye_la_url_del_anio_pedido()
    {
        Uri? pedida = null;
        var fuente = CrearFuente(request =>
        {
            pedida = request.RequestUri;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Fixtures.Leer(2026))
                {
                    Headers = { { "Content-Type", "application/json" } },
                },
            };
        });

        var result = await fuente.ObtenerAsync(2026);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            "https://www.argentina.gob.ar/sites/default/files/holidays-2026-es.json",
            pedida!.ToString()
        );
    }

    private static FuenteFeriadosArgentinaGobAr CrearFuente(
        Func<HttpRequestMessage, HttpResponseMessage> responder
    ) => new(
        new HttpClient(new HandlerFalso(responder)),
        Options.Create(new FeriadosNacionalesOptions()),
        NullLogger<FuenteFeriadosArgentinaGobAr>.Instance
    );

    private sealed class HandlerFalso : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public HandlerFalso(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        ) => Task.FromResult(_responder(request));
    }
}
