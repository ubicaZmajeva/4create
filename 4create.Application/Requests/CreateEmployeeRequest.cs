using System.Text.Json.Serialization;
using _4create.Application.Enums;
using MediatR;

namespace _4create.Application.Requests;

public class CreateEmployeeCommand: IRequest<int>
{
    public string Email { get; set; } = null!;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Titles Title { get; set; }
    
    public List<int> CompanyIds { get; set; } = new();
}