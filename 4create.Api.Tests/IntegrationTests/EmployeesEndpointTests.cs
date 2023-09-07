using System.Net;
using _4create.Api.Tests.IntegrationTests.Base;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace _4create.Api.Tests.IntegrationTests;

public class EmployeesEndpointTests : IntegrationTestBase
{
    public EmployeesEndpointTests(WebApplicationFactory<Program> factory)
        : base(factory) { }
    
    [Fact]
    public async Task EndpointExists()
    {
        var result = await Client.PostAsJsonAsync("/api/employees", new {});
        Assert.NotEqual(HttpStatusCode.NotFound, result.StatusCode);
    }
}