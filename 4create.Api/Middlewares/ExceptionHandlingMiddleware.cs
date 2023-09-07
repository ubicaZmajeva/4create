using System.Text.Json;
using FluentValidation;

namespace _4create.Api.Middlewares;

public class ExceptionHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            await HandleExceptionAsync(context, e);
        }
    }
    
    private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        var (statusCode, response) = 
            exception switch
            {
                ValidationException validationException => HandleValidationExceptions(validationException),
                _ => HandleGeneralExceptions(exception)
            };
        
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
    private static (int statusCodes, object response) HandleValidationExceptions(ValidationException ex) =>
        (StatusCodes.Status400BadRequest, 
            new
            {
                validationErrors = 
                    ex.Errors.Select(m => new
                    {
                        Property = m.PropertyName,
                        Error = m.ErrorMessage
                    })
            });

    private static (int statusCode, object response) HandleGeneralExceptions(Exception ex) => 
        (StatusCodes.Status500InternalServerError, $"Server Error: {ex.Message}");

}