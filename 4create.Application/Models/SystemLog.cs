using _4create.Application.Models.Base;

namespace _4create.Application.Models;

public class SystemLog: IBaseEntity
{
    public int Id { get; set; }
    public string ResourceType { get; set; } = null!;
    public int ResourceId { get; set; }
    public string Event { get; set; } = null!;
    public string ResourceAttributes { get; set; } = null!;
    public string Comment { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    public void Timestamp(DateTime dateTime)
    {
        CreatedAt = CreatedAt != default 
            ? CreatedAt 
            : dateTime;
    }
}