using _4create.Application.Enums;
using _4create.Application.Models.Base;

namespace _4create.Application.Models;

public class Employee: IAuditEntity
{
    public int Id { get;  set; }
    public Titles Title { get;  set; }
    public string Email { get; set; } = null!;
    
    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();

    public override string ToString() => $"Employee {Email}";
    public DateTime CreatedAt { get;  set; }
    public void Timestamp(DateTime dateTime)
    {
        CreatedAt = CreatedAt != default 
            ? CreatedAt 
            : dateTime;
    }
}