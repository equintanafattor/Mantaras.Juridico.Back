using System.Security.Claims;
using System.Text;
using Mantaras.Juridico.Application.Common.Interfaces;
using Mantaras.Juridico.Application.Features.Autenticacion.Services;
using Mantaras.Juridico.Application.Features.Usuarios.Services;
using Mantaras.Juridico.Infrastructure.Identity;
using Mantaras.Juridico.Infrastructure.Persistence;
using Mantaras.Juridico.Infrastructure.Persistence.Repositories;
using Mantaras.Juridico.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Mantaras.Juridico.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión DefaultConnection."
            );

        services.AddDbContext<JuridicoDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        services
            .AddIdentityCore<UsuarioIdentity>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<long>>()
            .AddEntityFrameworkStores<JuridicoDbContext>();
        var jwtIssuer =
            configuration[$"{JwtOptions.SectionName}:Issuer"]
            ?? throw new InvalidOperationException("No se configuró Jwt:Issuer.");

        var jwtAudience =
            configuration[$"{JwtOptions.SectionName}:Audience"]
            ?? throw new InvalidOperationException("No se configuró Jwt:Audience.");

        var jwtKey =
            configuration[$"{JwtOptions.SectionName}:Key"]
            ?? throw new InvalidOperationException("No se configuró Jwt:Key.");

        var expirationMinutes = int.TryParse(
            configuration[$"{JwtOptions.SectionName}:ExpirationMinutes"],
            out var configuredExpiration
        )
            ? configuredExpiration
            : 480;

        if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
        {
            throw new InvalidOperationException("Jwt:Key debe contener al menos 32 bytes.");
        }

        services.Configure<JwtOptions>(options =>
        {
            options.Issuer = jwtIssuer;
            options.Audience = jwtAudience;
            options.Key = jwtKey;
            options.ExpirationMinutes = expirationMinutes;
        });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var usuarioId = context.Principal?.FindFirstValue(
                            ClaimTypes.NameIdentifier
                        );

                        var tokenSecurityStamp = context.Principal?.FindFirstValue(
                            JwtClaimTypes.SecurityStamp
                        );

                        if (
                            string.IsNullOrWhiteSpace(usuarioId)
                            || string.IsNullOrWhiteSpace(tokenSecurityStamp)
                        )
                        {
                            context.Fail("El token no contiene los datos requeridos.");
                            return;
                        }

                        var userManager =
                            context.HttpContext.RequestServices
                                .GetRequiredService<UserManager<UsuarioIdentity>>();

                        var usuario = await userManager.FindByIdAsync(usuarioId);

                        if (usuario is null || !usuario.Activo)
                        {
                            context.Fail(
                                "El usuario no existe o se encuentra inactivo."
                            );
                            return;
                        }

                        var currentSecurityStamp =
                            await userManager.GetSecurityStampAsync(usuario);

                        if (
                            string.IsNullOrWhiteSpace(currentSecurityStamp)
                            || !string.Equals(
                                tokenSecurityStamp,
                                currentSecurityStamp,
                                StringComparison.Ordinal
                            )
                        )
                        {
                            context.Fail("La sesión ya no se encuentra vigente.");
                        }
                    }
                };
            });

        services.AddScoped<IAutenticacionService, AutenticacionService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<ICasoRepository, CasoRepository>();
        services.AddScoped<IExpedienteRepository, ExpedienteRepository>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUsuariosService, UsuariosService>();
        services.AddScoped<IPanelRepository, PanelRepository>();
        services.AddScoped<IObservacionRepository, ObservacionRepository>();

        services.AddScoped<ITipoBeneficioRepository, TipoBeneficioRepository>();
        services.AddScoped<ITipoExpedienteAdministrativoRepository, TipoExpedienteAdministrativoRepository>();

        return services;
    }
}
