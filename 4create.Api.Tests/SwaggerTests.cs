using System.Net;
using _4create.Api.Tests.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace _4create.Api.Tests.IntegrationTests;

public class SwaggerTests : IntegrationTestBase
{
    public SwaggerTests(WebApplicationFactory<Program> factory)
        : base(factory) { }

    [Theory]
    [InlineData("/swagger/v1/swagger.json")]
    [InlineData("/swagger/index.html")]
    public async Task ValidEndpoints_RespondWithOk(string endpoint)
    {
        var result = await Client.GetAsync(endpoint);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
    
}