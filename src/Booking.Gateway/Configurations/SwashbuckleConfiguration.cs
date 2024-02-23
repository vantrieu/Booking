using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Booking.Gateway.Configurations
{
    public static class SwashbuckleConfiguration
    {
        public static IServiceCollection AddSwashbuckle(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Booking Service",
                        Version = "v1"
                    }
                 );

                const string securityName = "Bearer";

                AddSecurityDefinition(securityName, c);

                AddSecurityRequirement(securityName, c);
            });

            return services;
        }

        public static IApplicationBuilder UseSwashbuckle(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.OAuthClientId("swagger");
                c.OAuthClientSecret("qO86bUF46EeHqGDNoV0QYyWo5ZMHSXVTSUJLJZdlE=");
                c.OAuthUsePkce();
                //c.SwaggerEndpoint("http://localhost:5000/swagger/v1/swagger.json", "Booking Service API");
                c.SwaggerEndpoint("http://localhost:5001/swagger/v1/swagger.json", "Payment Service API");
            });

            return app;
        }

        private static void AddSecurityDefinition(string securityName, SwaggerGenOptions options)
        {
            options.AddSecurityDefinition(securityName, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        TokenUrl = new Uri("http://localhost:5000/connect/token", UriKind.Absolute),
                        AuthorizationUrl = new Uri("http://localhost:5000/connect/authorize", UriKind.Absolute),
                        Scopes = new Dictionary<string, string> { { "swagger.api", "Swagger API Scope" } }
                    },
                },
            });
        }

        private static void AddSecurityRequirement(string securityName, SwaggerGenOptions options)
        {
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = securityName }
                    },
                    new List<string>{ "swagger.api" }
                }
            });
        }
    }
}
