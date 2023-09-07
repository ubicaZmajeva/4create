using _4create.Application;
using _4create.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using Xunit;

namespace _4create.Api.Tests.IntegrationTests.Base;

public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory
            .WithWebHostBuilder(builder => builder
                .ConfigureServices(services =>
                {
                    var context = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(AppDbContext));
                    if (context != null)
                    {
                        services.Remove(context);
                        var options = services.Where(r =>
                                r.ServiceType == typeof(DbContextOptions)
                                || (r.ServiceType.IsGenericType
                                    && r.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
                            .ToList();

                        foreach (var option in options)
                        {
                            services.Remove(option);
                        }
                    }

                    services.AddDbContext<IRepository, AppDbContext>(options =>
                        options.UseInMemoryDatabase("InMemoryDbForTesting")
                            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    );
                }));
        
        Client = Factory.CreateClient();
    }
    

    protected static async Task<List<(string propertName, string errorMessage)>> ExtractValidationExceptionsFromResponse(HttpResponseMessage result)
    {
        var responseJson = await result.Content.ReadAsStringAsync();
        dynamic? dynJson = JsonConvert.DeserializeObject(responseJson);
        var exceptions = new List<(string propertName, string errorMessage)>();
        if (dynJson == null) 
            return exceptions;
        
        foreach (var item in dynJson["validationErrors"])
        {
            exceptions.Add((item.Property, item.Error));
        }
        return exceptions;
    }
}