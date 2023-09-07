using System.Text.Json;
using System.Text.Json.Serialization;
using _4create.Application;
using _4create.Application.Enums;
using _4create.Application.Models;
using _4create.Application.Models.Base;
using Microsoft.EntityFrameworkCore;

namespace _4create.Infrastructure;

public partial class Repository: DbContext, IRepository
{
    public virtual DbSet<Company> Companies { get; set; } = null!;
    public virtual DbSet<Employee> Employees { get; set; } = null!;
    public virtual DbSet<SystemLog> SystemLogs { get; set; } = null!;

    public Repository(DbContextOptions<Repository> options): base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Repository).Assembly);
    }

    async Task<int?> IRepository.Persist<T>(T entity)
    {
        await using var dbContextTransaction = await Database.BeginTransactionAsync();
        try
        {
            var now = DateTime.Now;

            var auditLogs = await SaveEntity(entity, now);
            await SaveSystemLogs(auditLogs, now);
            
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
        var securityLogs = AddSecurityLogs();
        await SaveChangesAsync();
        return securityLogs;
    }

    private void AddCreatedAt(DateTime now)
    { 
        var entities = ChangeTracker
            .Entries()
            .Where(x => x is { Entity: IBaseEntity, State: EntityState.Added });

        foreach (var entity in entities)
        {
            if (entity.State == EntityState.Added)
                ((IBaseEntity)entity.Entity).Timestamp(now);
        }
    }

    private IEnumerable<SystemLogWithEntity> AddSecurityLogs() =>
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

    private async Task SaveSystemLogs(IEnumerable<SystemLogWithEntity> auditLogs, DateTime now)
    {
        var systemLogs = auditLogs.Select(m => m.PutResourceId()).ToList();
        await AddRangeAsync(systemLogs);
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

}