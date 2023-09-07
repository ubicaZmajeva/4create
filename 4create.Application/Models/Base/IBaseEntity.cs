namespace _4create.Application.Models.Base;

public interface IBaseEntity
{
    public int Id { get; set; }
    void CreateAtTimestamp(DateTime dateTime);
}