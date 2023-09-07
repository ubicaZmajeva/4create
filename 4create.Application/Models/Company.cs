using _4create.Application.Models.Base;

namespace _4create.Application.Models;

public class Company: IAuditEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public virtual ICollection<Employee> Employees { get;  set; } = new List<Employee>();

    public override string ToString() => $"Company {Name}";
    public DateTime CreatedAt { get; set; }
    public void Timestamp(DateTime dateTime)
    {
        CreatedAt = CreatedAt != default 
            ? CreatedAt 
            : dateTime;
    }
}