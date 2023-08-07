namespace UserService.Extensions.Services;

using UserService.Databases;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using HeimGuard;
using UserService.Services;
using UserService.Resources;
using Configurations;
using Microsoft.EntityFrameworkCore;
using UserService.Domain;
using System.Security.Claims;

public static class ServiceRegistration
{
    public static void AddInfrastructure(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
    {
        // DbContext -- Do Not Delete
        var connectionString = configuration.GetConnectionStringOptions().UserService;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new Exception("The database connection string is not set.");
        }

        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(connectionString,
                builder => builder.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName))
                            .UseSnakeCaseNamingConvention());

        services.AddHostedService<MigrationHostedService<UserDbContext>>();

    }
}
