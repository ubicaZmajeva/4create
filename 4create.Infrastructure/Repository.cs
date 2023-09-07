using _4create.Application.Enums;
using _4create.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace _4create.Infrastructure;

public partial class Repository
{
    public async Task<bool> EmployeeWithIdExists(string email, CancellationToken cancellationToken) => 
        await Set<Employee>()
            .AnyAsync(x => x.Email == email, cancellationToken);

    public async Task<bool> CompanyWithIdExists(CancellationToken cancellationToken, params int[] companyIds) => 
        await Set<Company>()
            .Where(m => companyIds.Contains(m.Id))
            .CountAsync(cancellationToken) == companyIds.Length;

    public async Task<bool> EmptyPositionInCompany(CancellationToken cancellationToken, Titles title, params int[] companyIds) =>
        !await Set<Company>()
            .Where(m => companyIds.Contains(m.Id))
            .SelectMany(m => m.Employees)
            .AnyAsync(m => m.Title == title, cancellationToken: cancellationToken);

    public async Task<bool> CompanyWithNameExists(CancellationToken cancellationToken, string name) => 
        await Set<Company>()
            .AnyAsync(x => x.Name == name, cancellationToken);

    public async Task<IEnumerable<Titles>> FetchTitlesForEmployees(CancellationToken cancellationToken, params int[] employeeIds) => 
        await Set<Employee>()
            .Where(m => employeeIds.Contains(m.Id))
            .Select(m => m.Title)
            .ToListAsync(cancellationToken);
    
    public Employee EmployeeById(int id)
    {
        var b = Entry(new Employee()
        {
            Id = id
        });

        b.State = EntityState.Unchanged;
        return b.Entity;

    }
    public Company CompanyById(int id)
    {
        var b = Entry(new Company()
        {
            Id = id
        });

        b.State = EntityState.Unchanged;
        return b.Entity;

    }
}