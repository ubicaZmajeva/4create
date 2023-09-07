using System.Diagnostics.CodeAnalysis;
using _4create.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySql.EntityFrameworkCore.Extensions;

namespace _4create.Infrastructure.Extensions;

[ExcludeFromCodeCoverage(Justification = "Configuration")]
public static class SupportInfrastructure
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMySQLServer<AppDbContext>(configuration.GetConnectionString(nameof(AppDbContext)) ?? throw new ApplicationException("Connection string is not configured"));
        services.AddTransient<IRepository, AppDbContext>();
        return services;
    }
    
    public static WebApplication EnsureDatabaseSchemaStructure(this WebApplication app)
    {     
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
        }
        else if (dbContext.Database.GetPendingMigrations().Any())
        {
            dbContext.Database.Migrate();
        }

        return app;
    }
}