using _4create.Application.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace _4create.Api.Endpoints;

public static class CompaniesEndpoints
{
    public static IEndpointRouteBuilder MapCompaniesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/companies")
            .MapPost(string.Empty,
                async ([FromBody] CreateCompanyCommand request,
                    IMediator mediator,
                    CancellationToken cancellationToken)  => 
                    Results.Ok(new
                    {
                        id = await mediator.Send(request, cancellationToken)
                    }))
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Creates a new Employee",
            });
        
        return app;
    }
}

