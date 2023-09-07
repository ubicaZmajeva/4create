using _4create.Application.Models.Base;

namespace _4create.Application.Models;

public class Company: IAuditEntity
{
    public static Company Create(string name) =>
        new()
        {
            Name = name
        };
    
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public virtual ICollection<Employee> Employees { get;  set; } = new List<Employee>();

    public override string ToString() => $"Company {Name}";
    public DateTime CreatedAt { get; internal set; }
    public void CreateAtTimestamp(DateTime dateTime)
    {
        CreatedAt = CreatedAt != default 
            ? CreatedAt 
            : dateTime;
    }
}