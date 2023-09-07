using System.Text.Json.Serialization;
using _4create.Api.Endpoints;
using _4create.Api.Extensions;
using _4create.Api.Middlewares;
using _4create.Application.Extensions;
using _4create.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddHandlers()
    .AddValidators()
    .Add4CreateSwagger()
    .AddPersistence(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddTransient<ExceptionHandlingMiddleware>();


var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapCompaniesEndpoints()
    .MapEmployeesEndpoints();

app.Use4CreateSwagger();
app.EnsureDatabaseSchemaStructure();
app.Run();

namespace _4create.Api
{
    public partial class Program { }
}