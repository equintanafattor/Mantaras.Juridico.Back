namespace Mantaras.Juridico.Application.Common.Authorization;

public static class RolesSistema
{
    public const string Administrador = "Administrador";

    public const string Usuario = "Usuario";

    public static readonly IReadOnlyCollection<string> Todos = new[] { Administrador, Usuario };
}
