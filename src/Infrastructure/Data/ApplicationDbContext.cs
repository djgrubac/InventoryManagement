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
    public DbSet<Category> Categories { get; set; }
    public DbSet<Warehouse> Wearhouses { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
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
        // builder.HasDefaultSchema("public");
        // builder.HasAnnotation("Relational:HistoryTableName", "__Efmigrationshistory");
        // builder.HasAnnotation("Relational:HistoryTableSchema", "public");
        
        // Configure Identity tables to use lowercase names
        builder.Entity<ApplicationUser>().ToTable("Aspnet_Users");
        builder.Entity<IdentityRole>().ToTable("Aspnet_Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("Aspnet_User_Roles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("Aspnet_User_Claims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("Aspnet_User_Logins");
        builder.Entity<IdentityUserToken<string>>().ToTable("Aspnet_User_Tokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("Aspnet_Role_Claims");

        base.OnModelCreating(builder);
        
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        builder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        builder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(pc => pc.Id);
            entity.Property(pc => pc.Caption).IsRequired();
        });
    }
}
