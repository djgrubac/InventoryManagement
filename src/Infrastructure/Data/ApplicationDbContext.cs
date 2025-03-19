using System.Reflection;
using Inventory_Management.Application.Common.Interfaces;
using Inventory_Management.Domain.Entities;
using Inventory_Management.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Management.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        builder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            builder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                
                entity.HasOne<ApplicationUser>() 
                    .WithMany()             
                    .HasForeignKey(u => u.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne<ProductCategory>()
                    .WithMany()
                    .HasForeignKey(pc => pc.ProductCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        });
        builder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(pc => pc.Id);
        });
    }
}
