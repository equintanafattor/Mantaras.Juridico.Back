using Mantaras.Juridico.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Mantaras.Juridico.Infrastructure.Persistence;

internal static class ViolacionUnicidad
{
    public static bool Es(DbUpdateException exception, out string? restriccion)
    {
        if (
            exception.InnerException is PostgresException postgres
            && postgres.SqlState == PostgresErrorCodes.UniqueViolation
        )
        {
            restriccion = postgres.ConstraintName;
            return true;
        }

        restriccion = null;
        return false;
    }

    // Guarda y traduce una violación de índice único a RegistroConcurrenteException.
    public static async Task GuardarAsync(
        DbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (Es(exception, out var restriccion))
        {
            throw new RegistroConcurrenteException(restriccion, exception);
        }
    }
}
