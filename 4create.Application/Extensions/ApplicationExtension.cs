using System.Diagnostics.CodeAnalysis;
using _4create.Application.Handlers.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace _4create.Application.Extensions;

[ExcludeFromCodeCoverage(Justification = "Configuration")]
public static class ApplicationExtension 
{
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddMediatR(m => m.RegisterServicesFromAssemblies(typeof(ApplicationExtension).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
    
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ApplicationExtension).Assembly);
        return services;
    }
}