using _4create.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _4create.Infrastructure.Configurations;

public class SystemLogEntityConfiguration: IEntityTypeConfiguration<SystemLog>
{
    public void Configure(EntityTypeBuilder<SystemLog> entity)
    {
        entity.ToTable("SystemLog");
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasKey(m => m.Id);
    
        entity.Property(e => e.ResourceType)
            .HasMaxLength(25);

        entity.Property(e => e.Event)
            .HasMaxLength(25);
        
    }
}