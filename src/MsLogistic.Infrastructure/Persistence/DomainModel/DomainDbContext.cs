using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Core.Abstractions;
using MsLogistic.Domain.Batches.Entities;
using MsLogistic.Domain.Customers.Entities;
using MsLogistic.Domain.DeliveryZones.Entities;
using MsLogistic.Domain.Drivers.Entities;
using MsLogistic.Domain.Orders.Entities;
using MsLogistic.Domain.Products.Entities;
using MsLogistic.Domain.Routes.Entities;

namespace MsLogistic.Infrastructure.Persistence.DomainModel;

internal class DomainDbContext : DbContext, IDatabase
{
    public DbSet<Batch> Batches { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<DeliveryZone> DeliveryZones { get; set; }
    public DbSet<Driver> Drivers { get; set; }
    public DbSet<OrderDelivery> OrderDeliveries { get; set; }
    public DbSet<OrderIncident> OrderIncidents { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Route> Routes { get; set; }

    public DomainDbContext(DbContextOptions<DomainDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Ignore<DomainEvent>();
        
        base.OnModelCreating(modelBuilder);
    }
    
    public void Migrate()
    {
        Database.Migrate();
    }
}