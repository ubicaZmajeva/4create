using _4create.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _4create.Infrastructure.Configurations;

internal class FRevenueConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> entity)
    {
        entity.ToTable("Company");
        
        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.HasKey(m => m.Id);
        
        entity.Property(m => m.Name)
            .IsUnicode()
            .HasMaxLength(50);

        entity.HasIndex(m => m.Name)
            .IsUnique();
        
        entity.HasMany(d => d.Employees)
            .WithMany(p => p.Companies)
            .UsingEntity<Dictionary<string, object>>(
                "CompaniesEmployeesBridge",
                r => r.HasOne<Employee>().WithMany()
                    .HasForeignKey("EmployeeId")
                    .OnDelete(DeleteBehavior.ClientSetNull),
                l => l.HasOne<Company>().WithMany()
                    .HasForeignKey("CompanyId")
                    .OnDelete(DeleteBehavior.ClientSetNull),
                m =>
                {
                    m.ToTable("CompaniesEmployeesBridge");
                });
        
    }
}