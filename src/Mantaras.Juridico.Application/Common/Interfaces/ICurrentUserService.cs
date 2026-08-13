namespace Mantaras.Juridico.Application.Common.Interfaces;

public interface ICurrentUserService
{
    long? UsuarioId { get; }

    string Usuario { get; }
}