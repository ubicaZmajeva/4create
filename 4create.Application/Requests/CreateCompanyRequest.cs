using System.Text.Json.Serialization;
using _4create.Application.Enums;
using MediatR;

namespace _4create.Application.Requests;

public class CreateCompanyCommand: IRequest<int>
{
    public string Name { get; set; } = null!;
    public List<Employee> Employees { get; set; } = new();
    
    public class Employee
    {
        public int? Id { get; set; }
        public string? Email { get; set; }
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Titles? Title { get; set; }
    }
}