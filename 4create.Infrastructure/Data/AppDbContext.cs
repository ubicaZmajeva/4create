using System.Text.Json;
using System.Text.Json.Serialization;
using _4create.Application;
using _4create.Application.Enums;
using _4create.Application.Models;
using _4create.Application.Models.Base;
using Microsoft.EntityFrameworkCore;

namespace _4create.Infrastructure;

public class AppDbContext: DbContext, IRepository
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    async Task<int?> IRepository.Persist<T>(T entity)
    {
        var now = DateTime.Now;
        await using var dbContextTransaction = await Database.BeginTransactionAsync();
        try
        {
            var systemLogs = await SaveEntity(entity, now);
            await SaveSystemLogs(systemLogs, now);
            
            await dbContextTransaction.CommitAsync();
            return entity.Id;
        }
        catch (Exception)
        {
            await dbContextTransaction.RollbackAsync();
            return null;
        }
    }

    private async Task<IEnumerable<SystemLogWithEntity>> SaveEntity<T>(T entity, DateTime now) where T : IBaseEntity
    {
        await AddAsync(entity);
        AddCreatedAt(now);
        var systemLogs = AddSystemLogs();
        await SaveChangesAsync();
        return systemLogs;
    }

    #region SystemLogs
    
    private void AddCreatedAt(DateTime now)
    { 
        var entities = ChangeTracker
            .Entries()
            .Where(x => x is { Entity: IBaseEntity, State: EntityState.Added });

        foreach (var entity in entities)
        {
            ((IBaseEntity)entity.Entity).CreateAtTimestamp(now);
        }
    }

    private IEnumerable<SystemLogWithEntity> AddSystemLogs() =>
        ChangeTracker
            .Entries()
            .Where(x => x is { Entity: IAuditEntity , State: EntityState.Added or EntityState.Modified or EntityState.Deleted})
            .ToList()
            .Select(entity => new SystemLogWithEntity()
            {
                Entity = (IAuditEntity)entity.Entity,
                ResourceType = entity.Entity.GetType().Name,
                Event = entity switch
                {
                    { State: EntityState.Deleted } => "Delete",
                    { State: EntityState.Modified } => "Modify",
                    { State: EntityState.Added } => "Create",
                    _ => "Manipulate"
                },
                ResourceAttributes = JsonSerializer.Serialize(entity.Entity,
                    new JsonSerializerOptions
                    {
                        ReferenceHandler = ReferenceHandler.Preserve
                    }),
                Comment = $"{entity.Entity} was {entity.State.ToString().ToLowerInvariant()}",
            })
            .ToList();

    private async Task SaveSystemLogs(IEnumerable<SystemLogWithEntity> systemLogs, DateTime now)
    {
        await AddRangeAsync(systemLogs.Select(m => m.PutResourceId()).ToList());
        AddCreatedAt(now);
        await SaveChangesAsync();
    }
    private class SystemLogWithEntity : SystemLog
    {
        public IAuditEntity Entity { private get; init; } = null!;

        public SystemLog PutResourceId()
        {
            ResourceId = Entity.Id;
            return this;
        }
    }
    
    #endregion SystemLogs
    
    #region Queries
    
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

    #endregion
    
    #region Entity Factory
    public T CreateEntity<T>(int id) where T : class, IBaseEntity, new()
    {
        var entry = Entry(new T
        {
            Id = id
        });
        entry.State = EntityState.Unchanged;
        return entry.Entity;
    }
    #endregion


}