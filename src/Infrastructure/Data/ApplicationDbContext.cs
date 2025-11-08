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
    public DbSet<Category> ProductCategories { get; set; }
    public DbSet<Warehouse> Wearhouses { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
    
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Configure all string properties to use citext (case-insensitive) by default
        configurationBuilder.Properties<string>().HaveColumnType("citext");
        
        base.ConfigureConventions(configurationBuilder);
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Configure schema and case-sensitivity for PostgreSQL
        builder.HasDefaultSchema("public");
        builder.HasAnnotation("Relational:HistoryTableName", "__efmigrationshistory");
        builder.HasAnnotation("Relational:HistoryTableSchema", "public");
        
        // Configure Identity tables to use lowercase names
        builder.Entity<ApplicationUser>().ToTable("aspnet_users");
        builder.Entity<IdentityRole>().ToTable("aspnet_roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("aspnet_user_roles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("aspnet_user_claims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("aspnet_user_logins");
        builder.Entity<IdentityUserToken<string>>().ToTable("aspnet_user_tokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("aspnet_role_claims");

        base.OnModelCreating(builder);
        
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Entity<Product>(entity =>
        {
            entity.ToTable("products"); // Add lowercase table name
            entity.HasKey(p => p.Id);
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<Category>()
                .WithMany()
                .HasForeignKey(pc => pc.ProductCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Category>(entity =>
        {
            entity.ToTable("product_categories"); // Add lowercase table name
            entity.HasKey(pc => pc.Id);
            entity.Property(pc => pc.Caption).IsRequired();
        });
    }
}
