namespace Mantaras.Juridico.Application.Features.Catalogos.Common;

public static class TiposOpcionCatalogo
{
    public const string FasesInternas = "fases-internas";
    public const string TiposTramite = "tipos-tramite";
    public const string EstadosLegales = "estados-legales";

    public static readonly IReadOnlyCollection<string> Todos =
        new[] { FasesInternas, TiposTramite, EstadosLegales };

    public static bool EsValido(string tipo) =>
        Todos.Contains(tipo, StringComparer.OrdinalIgnoreCase);
}
