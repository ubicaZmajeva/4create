using _4create.Api.Middlewares;
using FluentValidation;
using FluentValidation.Results;
using Xunit;

namespace _4create.Api.Tests;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task DelegateThrowsValidationExceptions_ReturnsBadRequest()
    {
        var httpContext = new DefaultHttpContext();
        var sus = new ExceptionHandlingMiddleware();
        
        RequestDelegate nextInChain = context => throw new ValidationException(new List<ValidationFailure>()
        {
            new("test", "test")
        });
        
        await sus.InvokeAsync(httpContext, nextInChain);

        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
    }
    
    
    [Fact]
    public async Task DelegateThrowsGenericException_Returns500()
    {
        var httpContext = new DefaultHttpContext();
        
        var sus = new ExceptionHandlingMiddleware();
        
        RequestDelegate nextInChain = context => throw new Exception();
        
        await sus.InvokeAsync(httpContext, nextInChain);

        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
    }
    
}