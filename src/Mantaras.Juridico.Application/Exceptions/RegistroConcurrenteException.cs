namespace Mantaras.Juridico.Application.Common.Exceptions;

// Un índice único de la base rechazó el guardado porque otra solicitud
// registró lo mismo al mismo tiempo. Los servicios lo usan para reintentar
// y devolver el resultado ya existente; si no se trata, la API responde 409.
public sealed class RegistroConcurrenteException : Exception
{
    public RegistroConcurrenteException(string? restriccion, Exception? innerException)
        : base("Otra solicitud registró los mismos datos al mismo tiempo.", innerException)
    {
        Restriccion = restriccion;
    }

    public string? Restriccion { get; }
}
