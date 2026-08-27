namespace Mantaras.Juridico.Application.Features.Catalogos.Exceptions;

// La infraestructura traduce únicamente la violación del índice único de Nombre.
// El servicio puede responder con un error de negocio sin depender de Npgsql.
public sealed class NombreCatalogoDuplicadoException : Exception
{
    public NombreCatalogoDuplicadoException(Exception innerException)
        : base("Ya existe un registro con el nombre informado.", innerException) { }
}
