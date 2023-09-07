using Microsoft.OpenApi.Models;

namespace _4create.Api.Extensions;

public static class SwaggerSupport
{
    public static IServiceCollection Add4CreateSwagger(this IServiceCollection services)
    {
        services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = $"4Create - task", 
                    });
            });
        
        return services;
    }
    
    public static IApplicationBuilder Use4CreateSwagger(this WebApplication app) =>
        app.UseSwagger().UseSwaggerUI();
}