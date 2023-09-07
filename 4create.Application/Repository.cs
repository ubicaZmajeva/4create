using _4create.Application.Enums;
using _4create.Application.Models.Base;

namespace _4create.Application;

public interface IRepository
{
    Task<int?> Persist<T>(T entity) where T: IBaseEntity;
    Task<bool> EmployeeWithIdExists(string email, CancellationToken cancellationToken);
    Task<bool> CompanyWithIdExists(CancellationToken cancellationToken, params int[] companyIds);
    Task<bool> EmptyPositionInCompany(CancellationToken cancellationToken, Titles title, params int[] companyIds);
    Task<bool> CompanyWithNameExists(CancellationToken cancellationToken, string name);
    Task<IEnumerable<Titles>> FetchTitlesForEmployees(CancellationToken cancellationToken, params int[] employeeIds);
    
    public T CreateEntity<T>(int id) where T : class, IBaseEntity, new();
}