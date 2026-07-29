using Mantaras.Juridico.Application.Common.Results;

namespace Mantaras.Juridico.Application.Features.Clientes;

public static class ClienteErrors
{
    public static readonly Error DniDuplicado = new(
        "Clientes.DniDuplicado",
        "Ya existe un cliente con el DNI informado."
    );

    public static readonly Error CuilDuplicado = new(
        "Clientes.CuilDuplicado",
        "Ya existe un cliente con el CUIL informado."
    );

    public static readonly Error NoEncontrado = new(
        "Clientes.NoEncontrado",
        "El cliente solicitado no existe."
    );
}
