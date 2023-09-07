using _4create.Application.Requests;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace _4create.Api.Endpoints;

public static class EmployeesEndpoints
{
    public static IEndpointRouteBuilder MapEmployeesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/employees")
            .MapPost(string.Empty, 
                async ([FromBody] CreateEmployeeCommand request, 
                    IMediator mediator, 
                    CancellationToken cancellationToken) => 
                    Results.Ok(new
                    {
                        id = await mediator.Send(request, cancellationToken)
                    }))
            .WithOpenApi(operation => new OpenApiOperation(operation)
            {
                Summary = "Creates a new Company",
            });
        
        return app;
    }
}