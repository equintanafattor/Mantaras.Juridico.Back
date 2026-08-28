using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Mantaras.Juridico.Infrastructure.Persistence;

public class JuridicoDbContextFactory
    : IDesignTimeDbContextFactory<JuridicoDbContext>
{
    public JuridicoDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        var apiDirectory = Directory.Exists(
            Path.Combine(currentDirectory, "Mantaras.Juridico.Api"))
            ? Path.Combine(currentDirectory, "Mantaras.Juridico.Api")
            : currentDirectory;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDirectory)
            .AddJsonFile(
                "appsettings.json",
                optional: false)
            .AddJsonFile(
                "appsettings.Development.json",
                optional: true)
            .AddUserSecrets(
                "9f13f861-1619-4119-99b6-f287886ea1d7",
                reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión DefaultConnection.");

        var optionsBuilder =
            new DbContextOptionsBuilder<JuridicoDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new JuridicoDbContext(optionsBuilder.Options);
    }
}