using Mantaras.Juridico.Application.Common.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mantaras.Juridico.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task InicializarAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration
    )
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<long>>>();

        foreach (var roleName in RolesSistema.Todos)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var roleResult = await roleManager.CreateAsync(new IdentityRole<long>(roleName));

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    ConstruirMensajeError($"No se pudo crear el rol {roleName}", roleResult.Errors)
                );
            }
        }

        var email = configuration["BootstrapAdmin:Email"];
        var password = configuration["BootstrapAdmin:Password"];
        var nombre = configuration["BootstrapAdmin:Nombre"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var userManager = serviceProvider.GetRequiredService<UserManager<UsuarioIdentity>>();

        email = email.Trim();

        var usuario = await userManager.FindByEmailAsync(email);

        if (usuario is null)
        {
            usuario = new UsuarioIdentity
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                Nombre = string.IsNullOrWhiteSpace(nombre) ? "Administrador" : nombre.Trim(),
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                LockoutEnabled = true,
            };

            var createResult = await userManager.CreateAsync(usuario, password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    ConstruirMensajeError(
                        "No se pudo crear el administrador inicial",
                        createResult.Errors
                    )
                );
            }
        }

        if (!await userManager.IsInRoleAsync(usuario, RolesSistema.Administrador))
        {
            var roleResult = await userManager.AddToRoleAsync(usuario, RolesSistema.Administrador);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    ConstruirMensajeError(
                        "No se pudo asignar el rol Administrador",
                        roleResult.Errors
                    )
                );
            }
        }
    }

    private static string ConstruirMensajeError(string mensaje, IEnumerable<IdentityError> errors)
    {
        var detalle = string.Join(" ", errors.Select(error => error.Description));

        return $"{mensaje}. {detalle}";
    }
}
