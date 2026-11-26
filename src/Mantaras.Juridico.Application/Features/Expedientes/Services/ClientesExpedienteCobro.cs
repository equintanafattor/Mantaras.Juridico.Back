using Mantaras.Juridico.Application.Features.Expedientes.Responses;
using Mantaras.Juridico.Domain.Entities;

namespace Mantaras.Juridico.Application.Features.Expedientes.Services;

// Clientes de un judicial y su cliente principal, a partir de Expediente.Casos → Caso.Clientes
// (ya cargados por la consulta del listado: no hace consultas propias).
//
// Se consideran todos los casos vinculados y todos sus clientes, incluidos los dados de baja,
// para no ocultar la asociación; cada cliente informa su Activo.
//
// Cliente principal:
//   1. Si el judicial tiene un único cliente distinto, ese.
//   2. Si no, el único cliente distinto marcado EsPrincipal en alguno de sus casos.
//   3. Si no hay clientes, o hay 0 o más de un principal distinto, null: es ambiguo
//      y el front ofrece elegir entre los clientes devueltos.
public static class ClientesExpedienteCobro
{
    public static (IReadOnlyCollection<ClienteExpedienteCobroResponse> Clientes, long? ClientePrincipalId)
        Resolver(Expediente expediente)
    {
        var clientes = expediente.Casos
            .SelectMany(casoExpediente => casoExpediente.Caso.Clientes)
            .GroupBy(casoCliente => casoCliente.ClienteId)
            .Select(grupo =>
            {
                var cliente = grupo.First().Cliente;

                return new ClienteExpedienteCobroResponse
                {
                    ClienteId = grupo.Key,
                    Nombre = cliente.Nombre,
                    Apellido = cliente.Apellido,
                    NombreCompleto = $"{cliente.Apellido}, {cliente.Nombre}",
                    Dni = cliente.Dni,
                    Cuil = cliente.Cuil,
                    Activo = cliente.Activo,
                    EsPrincipal = grupo.Any(casoCliente => casoCliente.EsPrincipal),
                };
            })
            .OrderByDescending(x => x.EsPrincipal)
            .ThenBy(x => x.Apellido)
            .ThenBy(x => x.Nombre)
            .ThenBy(x => x.ClienteId)
            .ToArray();

        if (clientes.Length == 1)
        {
            return (clientes, clientes[0].ClienteId);
        }

        var principales = clientes.Where(x => x.EsPrincipal).ToArray();

        return (clientes, principales.Length == 1 ? principales[0].ClienteId : null);
    }
}
