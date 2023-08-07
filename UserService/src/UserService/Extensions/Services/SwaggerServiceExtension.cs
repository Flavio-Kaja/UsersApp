namespace UserService.Extensions.Services;

using UserService.Services;
using Configurations;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

public static class SwaggerServiceExtension
{
    public static void AddSwaggerExtension(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(config =>
        {
            config.CustomSchemaIds(type => type.ToString());
            config.MapType<DateOnly>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date"
            });

            config.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Version = "v1",
                    Title = "",
                    Description = "",
                    Contact = new OpenApiContact
                    {
                        Name = "",
                        Email = "",
                    },
                });
            //authorization using Swagger (JWT)
            config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter ‘Bearer’ [space] and then your valid token in the text input below." +
                              "\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\""
            });
            config.AddSecurityRequirement(new OpenApiSecurityRequirement
                {{
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }});
            config.IncludeXmlComments(string.Format(@$"{AppDomain.CurrentDomain.BaseDirectory}{Path.DirectorySeparatorChar}UserService.WebApi.xml"));
        });
    }
}