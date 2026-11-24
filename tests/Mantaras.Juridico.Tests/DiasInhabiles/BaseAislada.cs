using Mantaras.Juridico.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mantaras.Juridico.Tests.DiasInhabiles;

// Las pruebas con base corren solo si MANTARAS_TEST_DB apunta a una base cuyo
// nombre contiene "test". Nunca usar la base de desarrollo: se vacían tablas.
public sealed class FactConBaseAttribute : FactAttribute
{
    public FactConBaseAttribute()
    {
        if (BaseAislada.CadenaConexion is null)
        {
            Skip = "Definir MANTARAS_TEST_DB con una base PostgreSQL aislada (el nombre debe contener 'test').";
        }
    }
}

public sealed class BaseAislada : IAsyncLifetime
{
    public static string? CadenaConexion
    {
        get
        {
            var cadena = Environment.GetEnvironmentVariable("MANTARAS_TEST_DB");

            if (string.IsNullOrWhiteSpace(cadena))
            {
                return null;
            }

            var baseDatos = new NpgsqlConnectionStringBuilder(cadena).Database ?? "";

            return baseDatos.Contains("test", StringComparison.OrdinalIgnoreCase) ? cadena : null;
        }
    }

    public JuridicoDbContext CrearContexto() => new(
        new DbContextOptionsBuilder<JuridicoDbContext>()
            .UseNpgsql(CadenaConexion!)
            .Options
    );

    public async Task InitializeAsync()
    {
        if (CadenaConexion is null)
        {
            return;
        }

        await using var contexto = CrearContexto();
        await contexto.Database.MigrateAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public async Task VaciarAsync()
    {
        await using var contexto = CrearContexto();
        await contexto.Database.ExecuteSqlRawAsync(
            "TRUNCATE \"DiasInhabiles\", \"CalendariosFeriadosImportados\" RESTART IDENTITY;"
        );
    }
}

[CollectionDefinition(Nombre)]
public sealed class ColeccionBaseAislada : ICollectionFixture<BaseAislada>
{
    public const string Nombre = "BaseAislada";
}
