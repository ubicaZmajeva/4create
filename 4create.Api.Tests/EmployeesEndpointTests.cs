using System.Net;
using _4create.Api.Tests.IntegrationTests.Base;
using _4create.Application;
using _4create.Application.Enums;
using _4create.Application.Models;
using _4create.Infrastructure;
using AutoFixture;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Xunit;

namespace _4create.Api.Tests.IntegrationTests;

public class EmployeesEndpointTests : IntegrationTestBase
{
    private readonly Fixture _fixture;
    
    public EmployeesEndpointTests(WebApplicationFactory<Program> factory)
        : base(factory)
    {
        _fixture = new Fixture();
    }
    
    [Fact]
    public async Task CreateCompany_EndpointExists()
    {
        var result = await Client.PostAsJsonAsync("/api/employees", new {});
        Assert.NotEqual(HttpStatusCode.NotFound, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateCompany_Success()
    {
        using var scope = Factory.Services.CreateScope();
        var context = (AppDbContext)scope.ServiceProvider.GetRequiredService<IRepository>();

        var email = $"{_fixture.Create<string>()}@a.com";
        var result = await Client.PostAsJsonAsync("/api/employees", new
        {
            email,
            Title = Titles.Manager
        });
        
        var content = await result.Content.ReadAsStringAsync();
        var id = JObject.Parse(content).Value<int>("id");
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(await context.Set<Employee>().FindAsync(id));
        Assert.NotNull(await context.Set<SystemLog>().Where(m => m.ResourceId == id && m.ResourceType == "Employee").FirstOrDefaultAsync());
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("")]
    public async Task CreateCompany_EmptyEmail_ReturnsBadRequest(string? email)
    {
        var result = await Client.PostAsJsonAsync("/api/employees", new
        {
            email,
            Title = Titles.Manager
        });
        
        var exceptionMessages = await ExtractValidationExceptionsFromResponse(result);
        Assert.Contains(("Email", "'Email' must not be empty."), exceptionMessages);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateCompany_NotAnEmail_ReturnsBadRequest()
    {
        var email = _fixture.Create<string>();
        var result = await Client.PostAsJsonAsync("/api/employees", new
        {
            email,
            Title = Titles.Manager
        });
        
        var exceptionMessages = await ExtractValidationExceptionsFromResponse(result);
        Assert.Contains(("Email", "'Email' is not a valid email address."), exceptionMessages);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateCompany_DuplicateEmail_ReturnsBadResult()
    {
        var email = $"{_fixture.Create<string>()}@a.com";
        var result = await Client.PostAsJsonAsync("/api/employees", new
        {
            email,
            Title = Titles.Manager
        });
        result.EnsureSuccessStatusCode();
        
        result = await Client.PostAsJsonAsync("/api/employees", new
        {
            email,
            Title = Titles.Manager
        });
        
        var exceptionMessages = await ExtractValidationExceptionsFromResponse(result);
        Assert.Contains(("Email", "Employee with this email already exists."), exceptionMessages);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateCompany_NoTitle_ReturnsBadResult()
    {
        var email = $"{_fixture.Create<string>()}@a.com";
        
        var result = await Client.PostAsJsonAsync("/api/employees", new
        {
            email,
        });
        
        var exceptionMessages = await ExtractValidationExceptionsFromResponse(result);
        Assert.Contains(("Title", "'Title' must not be empty."), exceptionMessages);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateCompany_WrongTitle_ReturnsBadResult()
    {
        var email = $"{_fixture.Create<string>()}@a.com";
        var result = await Client.PostAsJsonAsync("/api/employees", new
        {
            email,
            title = 5
        });
        
        var exceptionMessages = await ExtractValidationExceptionsFromResponse(result);
        Assert.Contains(("Title", "'Title' has a range of values which does not include '5'."), exceptionMessages);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateCompany_DuplicateCompanyIds_ReturnsBadResult()
    {
        var email = $"{_fixture.Create<string>()}@a.com";
        var result = await Client.PostAsJsonAsync("/api/employees", new
        {
            email,
            title = Titles.Manager,
            companyIds = new List<int>{1, 1}
        });
        
        var exceptionMessages = await ExtractValidationExceptionsFromResponse(result);
        Assert.Contains(("CompanyIds", "Duplicate company ids are not allowed."), exceptionMessages);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task CreateCompany_CompanyIdDoesNotExist_ReturnsBadResult()
    {
        var email = $"{_fixture.Create<string>()}@a.com";
        var result = await Client.PostAsJsonAsync("/api/employees", new
        {
            email,
            title = Titles.Manager,
            companyIds = new List<int>{ -1 }
        });
        
        var exceptionMessages = await ExtractValidationExceptionsFromResponse(result);
        Assert.Contains(("CompanyIds", "One of specified companies does not exist."), exceptionMessages);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateCompany_EmployeeWIthSameTitleExists_ReturnsBadResult()
    {
        var result = await Client.PostAsJsonAsync("/api/companies", new
        {
           Name = _fixture.Create<string>()
        });
        result.EnsureSuccessStatusCode();
        
        var content = await result.Content.ReadAsStringAsync();
        var companyId = JObject.Parse(content).Value<int>("id");
        
        result = await Client.PostAsJsonAsync("/api/employees", new
        {
            email = $"{_fixture.Create<string>()}@a.com",
            Title = Titles.Manager,
            companyIds = new List<int>() { companyId }
        });
        result.EnsureSuccessStatusCode();
        
        result = await Client.PostAsJsonAsync("/api/employees", new
        {
            email = $"{_fixture.Create<string>()}@b.com",
            Title = Titles.Manager,
            companyIds = new List<int>() { companyId }
        });
        
        var exceptionMessages = await ExtractValidationExceptionsFromResponse(result);
        Assert.Contains(("CompanyIds", "One of specified companies does not have empty position with this title."), exceptionMessages);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }


    
    
    
    
}