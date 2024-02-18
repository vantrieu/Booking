using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using IdentityModel;
using Booking.Authenticate.Common.Enums;
using Booking.Authenticate.Common.Extensions;
using Booking.Authenticate.Data;
using Booking.Authenticate.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Booking.Authenticate;

public class SeedData
{
    public static void EnsureSeedApplicationData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
            
            context.Database.Migrate();

            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            CreateRoleAsync(Role.Administrator.GetDescription(), roleMgr);

            CreateRoleAsync(Role.TenantAdmin.GetDescription(), roleMgr);

            CreateRoleAsync(Role.TenantOperator.GetDescription(), roleMgr);

            CreateRoleAsync(Role.TenantStaff.GetDescription(), roleMgr);

            CreateRoleAsync(Role.TenantUser.GetDescription(), roleMgr);

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var administrator = userMgr.FindByNameAsync("administrator").Result;

            if (administrator == null)
            {
                administrator = new ApplicationUser
                {
                    UserName = "administrator",
                    Email = "trieunv.it@gmail.com",
                    EmailConfirmed = true
                };

                administrator.Tenant = new Tenant
                {
                    ApplicationUserId = administrator.Id,
                    OwnerId = administrator.Id
                };

                administrator.TenantId = administrator.Tenant.Id;

                var result = userMgr.CreateAsync(administrator, "Luc1us!Admin@123").Result;

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                userMgr.AddToRolesAsync(administrator, new[] { Role.Administrator.GetDescription(),
                    Role.TenantAdmin.GetDescription(), Role.TenantOperator.GetDescription(),
                    Role.TenantStaff.GetDescription(), Role.TenantUser.GetDescription()}).Wait();

                Log.Information("Create \"Administrator\" succesfully.");
            }
            else
            {
                Log.Debug("\"Administrator\" already exists.");
            }
        }
    }

    private static void CreateRoleAsync(string roleName, RoleManager<IdentityRole> roleManager)
    {
        if (!roleManager.RoleExistsAsync(roleName).Result)
        {
            var role = new IdentityRole(roleName);

            roleManager.CreateAsync(role).Wait();
        }
    }

    public static void EnsureSeedDuendeIdentityData(WebApplication app)
    {
        var configuration = app.Configuration;

        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var configurationDbContext = scope.ServiceProvider.GetService<ConfigurationDbContext>();

            configurationDbContext.Database.Migrate();

            Log.Information($"Seed \"ConfigurationDbContext\" successfully.");

            var persistedGrantDbContext = scope.ServiceProvider.GetService<PersistedGrantDbContext>();

            persistedGrantDbContext.Database.Migrate();

            Log.Information($"Seed \"PersistedGrantDbContext\" successfully.");

            var clients = new List<Client>();

            if (!configurationDbContext.Clients.Any(x => x.ClientId == "booking"))
            {
                clients.Add(new Client
                {
                    ClientId = "booking",
                    ClientSecrets = { new Secret("rH4XnTDhFpvhDHGqj9wTWm3Q3hr8cOo1EOMxbR4E=".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireClientSecret = true,
                    RequirePkce = false,
                    RedirectUris = { configuration["IdentityDbConfig:Booking:RedirectUris"] },
                    PostLogoutRedirectUris = { configuration["IdentityDbConfig:Booking:PostLogoutRedirectUris"] },
                    AllowedCorsOrigins = { configuration["IdentityDbConfig:Booking:AllowedCorsOrigins"] },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        IdentityServerConstants.StandardScopes.Phone,
                        "booking.api"
                    },
                    AccessTokenLifetime = 3600,
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = true,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly,
                    RefreshTokenExpiration = TokenExpiration.Sliding,
                    AccessTokenType = AccessTokenType.Reference
                });
            }

            if (!configurationDbContext.Clients.Any(x => x.ClientId == "swagger"))
            {
                clients.Add(new Client
                {
                    ClientId = "swagger",
                    ClientSecrets = { new Secret("qO86bUF46EeHqGDNoV0QYyWo5ZMHSXVTSUJLJZdlE=".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,

                    RequireConsent = false,
                    RequirePkce = true,

                    RedirectUris = configuration["IdentityDbConfig:Swagger:RedirectUris"].Split(" ").ToList(),
                    PostLogoutRedirectUris = configuration["IdentityDbConfig:Swagger:PostLogoutRedirectUris"].Split(" ").ToList(),
                    AllowedCorsOrigins = configuration["IdentityDbConfig:Swagger:AllowedCorsOrigins"].Split(" ").ToList(),

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "swagger.api"
                    }
                });
            }

            if (!configurationDbContext.IdentityResources.Any())
            {
                var identityResources = new List<IdentityResource>()
                {
                    new IdentityResources.OpenId(),
                    new IdentityResources.Profile(),
                    new IdentityResources.Email(),
                    new IdentityResources.Phone()
                };

                configurationDbContext.IdentityResources.AddRange(identityResources.Select(x => x.ToEntity()));

                configurationDbContext.SaveChanges();

                Log.Information("Seed \"IdentityResources\" successfully.");
            }

            if (!configurationDbContext.ApiScopes.Any())
            {
                var apiScopes = new List<ApiScope>()
                {
                    new ApiScope("booking.api", "Booking Service API"),
                    new ApiScope("swagger.api", "Swagger Service API")
                };

                configurationDbContext.ApiScopes.AddRange(apiScopes.Select(x => x.ToEntity()));

                configurationDbContext.SaveChanges();

                Log.Information("Seed \"ApiScopes\" successfully.");
            }

            if (!configurationDbContext.ApiResources.Any())
            {
                var apiResources = new List<ApiResource>
                {
                    new ApiResource("booking.api", "Booking Service API", new[] { "roles" })
                    {
                        Scopes = { "booking.api" },
                    },
                    new ApiResource("swagger.api", "Swagger Service API", new[] { "roles" })
                    {
                        Scopes = { "swagger.api" },
                    }
                };

                configurationDbContext.ApiResources.AddRange(apiResources.Select(x => x.ToEntity()));

                configurationDbContext.SaveChanges();

                Log.Information("Seed \"ApiResources\" successfully.");
            }

            if (clients.Any())
            {
                configurationDbContext.Clients.AddRange(clients.Select(x => x.ToEntity()));

                configurationDbContext.SaveChanges();

                Log.Information("Seed \"Clients\" successfully.");
            }
        }
    }
}
