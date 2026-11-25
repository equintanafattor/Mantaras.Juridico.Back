using Mantaras.Juridico.Application.Features.Catalogos.Common;
using Mantaras.Juridico.Infrastructure.Persistence.Migrations;
using Mantaras.Juridico.Tests.DiasInhabiles;
using Npgsql;

namespace Mantaras.Juridico.Tests.Juzgados;

// La migración AgregarCatalogoJuzgados unifica los textos libres en SQL. Si su
// normalización difiriera de NombreCatalogo.Normalizar, la API no detectaría como
// duplicado un nombre que la migración ya creó.
public class NormalizacionMigracionJuzgadosTests
{
    public static TheoryData<string> Textos => new()
    {
        "Juzgado Federal N° 2 de Paraná",
        "  JUZGADO   FEDERAL n° 2 DE PARANÁ  ",
        "juzgado federal n° 2 de paraná",
        "Juzgado\u00A0Federal\tN° 2\r\nde\u2003Paraná\u3000",
        "juzgado de la niñez de santa fe",
        "cámara federal de apelaciones — sala ü ç",
        "Juzgado Federal N.º 1",
        "   ",
        "",
    };

    [Theory]
    [MemberData(nameof(Textos))]
    public async Task Sql_de_la_migracion_normaliza_igual_que_la_api(string texto)
    {
        if (BaseAislada.CadenaConexion is null)
        {
            return; // Sin base aislada configurada (ver MANTARAS_TEST_DB).
        }

        await using var conexion = new NpgsqlConnection(BaseAislada.CadenaConexion);
        await conexion.OpenAsync();

        var esperado = NombreCatalogo.Normalizar(texto);

        // Locale de la base y collation "C" (upper() sin soporte de acentos).
        foreach (var columna in new[] { "@v", "(@v COLLATE \"C\")" })
        {
            await using var comando = new NpgsqlCommand(
                $"SELECT {AgregarCatalogoJuzgados.NormalizarSql(columna)}",
                conexion
            );
            comando.Parameters.AddWithValue("v", texto);

            var obtenido = (string?)await comando.ExecuteScalarAsync();

            Assert.Equal(esperado, obtenido);
        }
    }
}
