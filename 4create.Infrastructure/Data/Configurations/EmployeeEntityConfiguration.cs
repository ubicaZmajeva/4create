using _4create.Application.Enums;
using _4create.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace _4create.Infrastructure.Configurations;

public class EmployeeEntityConfiguration: IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> entity)
    {
        entity.ToTable("Employee");
        
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasKey(m => m.Id);
        
        entity.Property(m => m.Title)
            .HasConversion(new EnumToStringConverter<Titles>());
        
        entity.Property(m => m.Email)
            .IsUnicode()
            .HasMaxLength(250);
        
        entity.HasIndex(m => m.Email)
            .IsUnique();
    }
}