namespace UserService.Extensions.Services;

using UserService.Middleware;
using UserService.Services;
using Configurations;
using System.Text.Json.Serialization;
using Serilog;
using FluentValidation.AspNetCore;
using Hellang.Middleware.ProblemDetails;
using Hellang.Middleware.ProblemDetails.Mvc;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Resources;
using Sieve.Services;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Users;
using UserService.Domain.Roles;
using UserService.Databases;
using System.Data.Entity;
using Microsoft.AspNetCore.Identity;
using FluentValidation;
using UserService.Domain.RolePermissions.Dtos;
using UserService.Authentication.Models;
using Microsoft.AspNet.Identity;
using UserService.Domain.Users.Dtos;

public static class WebAppServiceConfiguration
{
    public static void ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();
        builder.Services.AddSingleton(Log.Logger);
        builder.Services.AddProblemDetails(ProblemDetailsConfigurationExtension.ConfigureProblemDetails)
            .AddProblemDetailsConventions();

        builder.Services.AddCorsService("UserServiceCorsPolicy", builder.Environment);
        builder.OpenTelemetryRegistration(builder.Configuration, "UserService");
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddIdentity<User, Role>()
               .AddEntityFrameworkStores<UserDbContext>()
               .AddDefaultTokenProviders();
        builder.Services.AddInfrastructure(builder.Environment, builder.Configuration);
        //Added authentication
        builder.Services.AddAppAuthentication(builder.Environment, builder.Configuration);
        builder.Services.AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
        builder.Services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.AssumeDefaultVersionWhenUnspecified = true;
            config.ReportApiVersions = true;
        });

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        builder.Services.AddScoped<SieveProcessor>();
        builder.Services.AddValidatorsFromAssemblyContaining<PostRolePermissionDto>();

        builder.Services.AddScoped<IValidator<PostRolePermissionDto>, PostRolePermissionDtoValidator>();
        builder.Services.AddScoped<IValidator<UserLoginModel>, UserLoginValidator>();
        builder.Services.AddScoped<IValidator<PostUserDto>, PostUserDtoValidator>();

        builder.Services.AddMvc().AddFluentValidation(op => op.AutomaticValidationEnabled = false);


        builder.Services.AddBoundaryServices(Assembly.GetExecutingAssembly());


        builder.Services.AddHealthChecks();
        builder.Services.AddSwaggerExtension(builder.Configuration);
    }

    /// <summary>
    /// Registers all services in the assembly of the given interface.
    /// </summary>
    private static void AddBoundaryServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (!assemblies.Any())
            throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");

        foreach (var assembly in assemblies)
        {
            var rules = assembly.GetTypes()
                .Where(x => !x.IsAbstract && x.IsClass && x.GetInterface(nameof(IUserServiceScopedService)) == typeof(IUserServiceScopedService));

            foreach (var rule in rules)
            {
                foreach (var @interface in rule.GetInterfaces())
                {
                    services.Add(new ServiceDescriptor(@interface, rule, ServiceLifetime.Scoped));
                }
            }
        }
    }
}