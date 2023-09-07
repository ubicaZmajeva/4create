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

public class CompaniesEndpointTests : IntegrationTestBase
{
    private readonly Fixture _fixture;
    
    public CompaniesEndpointTests(WebApplicationFactory<Program> factory)
        : base(factory)
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateCompany_EndpointExists()
    {
        var result = await Client.PostAsJsonAsync("/api/companies", new {});
        Assert.NotEqual(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task CreateCompany_Success()
    {
        using var scope = Factory.Services.CreateScope();
        var context = (AppDbContext)scope.ServiceProvider.GetRequiredService<IRepository>();

        var companyName = _fixture.Create<string>();
        var result = await Client.PostAsJsonAsync("/api/companies", new
        {
            name = companyName,
            employees = new List<object>
            {
                new 
                {
                    Email = $"{_fixture.Create<string>()}@a.com",
                    Title = Titles.Developer
                },
                new 
                {
                    Email = $"{_fixture.Create<string>()}@a.com",
                    Title = Titles.Manager
                }
            }
        });
        
        var content = await result.Content.ReadAsStringAsync();
        var id = JObject.Parse(content).Value<int>("id");
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(await context.Set<Company>().FindAsync(id));
        Assert.NotNull(await context.Set<SystemLog>().Where(m => m.ResourceId == id && m.ResourceType == "Company").FirstOrDefaultAsync());
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("")]
    public async Task CreateCompany_EmptyName_ReturnsBadRequest(string? name)
    {
        var result = await Client.PostAsJsonAsync("/api/companies", new { name });
        var exceptionMessages = await ExtractValidationExceptionsFromResponse(result);
        Assert.Contains(("Name", "'Name' must not be empty."), exceptionMessages);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateCompany_50PlusCharsName_ReturnsBadRequest()
    {
        var name = Guid.NewGuid().ToString().PadRight(51, '±');
        var result = await Client.PostAsJsonAsync("/api/companies", new { name });
        var exceptionMessages = await ExtractValidationExceptionsFromResponse(result);
        Assert.Contains(("Name", "The length of 'Name' must be 50 characters or fewer. You entered 51 characters."), exceptionMessages);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateCompany_CompanyWithExistingName_ReturnsBadRequest()
    {
        var name = _fixture.Create<string>();
        var result = await Client.PostAsJsonAsync("/api/companies", new { name });
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        
        result = await Client.PostAsJsonAsync("/api/companies", new { name });
        var exceptionMessages = await ExtractValidationExceptionsFromResponse(result);
        Assert.Contains(("Name", "Company with this name already exists."), exceptionMessages);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateCompany_NeitherIdNorEmailOfEmployeeIsSpecified_ReturnsBadRequest()
    {
        var name = _fixture.Create<string>();
        var result = await Client.PostAsJsonAsync("/api/companies", new
        {
            name,
            Employees = new List<object>
            {
                new { }
            }
        });
        var exceptionMessages = await ExtractValidationExceptionsFromResponse(result);
        Assert.Contains(("Employees[0]", "Either Id of existing employee or Email and Title of new employee must be provide (but not both)."), exceptionMessages);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateCompany_BothIdAndEmailOfEmployeeIsSpecified_ReturnsBadRequest()
    {
        var name = _fixture.Create<string>();
        var result = await Client.PostAsJsonAsync("/api/companies", new
        {
            name,
            Employees = new List<object>
            {
                new
                {
                    Id = _fixture.Create<int>(),
                    Email = _fixture.Create<string>(),
                    Title = Titles.Developer.ToString()
                }
            }
        });
        var exceptionMessages = await ExtractValidationExceptionsFromResponse(result);
        Assert.Contains(("Employees[0]", "Either Id of existing employee or Email and Title of new employee must be provide (but not both)."), exceptionMessages);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task CreateCompany_EmployeesWithSameTitleInRequest_ReturnsBadRequest()
    {
        var name = _fixture.Create<string>();
        var result = await Client.PostAsJsonAsync("/api/companies", new
        {
            name,
            Employees = new List<object>
            {
                new 
                {
                    Email = _fixture.Create<string>(),
                    Title = Titles.Developer
                },
                new
                {
                    Email = _fixture.Create<string>(),
                    Title = Titles.Developer
                }
            }
        });
        var exceptionMessages = await ExtractValidationExceptionsFromResponse(result);
        Assert.Contains(("Employees", "Duplicate employee titles are not allowed."), exceptionMessages);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }
}