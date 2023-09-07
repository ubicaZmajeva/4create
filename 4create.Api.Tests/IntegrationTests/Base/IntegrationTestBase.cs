using _4create.Application;
using _4create.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
                    var context = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(Repository));
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

                    services.AddDbContext<IRepository, Repository>(options =>
                        options.UseInMemoryDatabase("InMemoryDbForTesting")
                            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    );
                }));
        
        Client = Factory.CreateClient();
    }
}