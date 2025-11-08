using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MsLogistic.Core.Abstractions;
using MsLogistic.Domain.Customer.Entities;
using MsLogistic.Domain.DeliveryPerson.Entities;
using MsLogistic.Domain.DeliveryZone.Entities;
using MsLogistic.Domain.Order.Entities;
using MsLogistic.Domain.Product.Entities;
using MsLogistic.Domain.Route.Entities;

namespace MsLogistic.Infrastructure.Persistence.DomainModel;

internal class DomainDbContext : DbContext
{
    public DbSet<Customer> Customer { get; set; }
    public DbSet<DeliveryPerson> DeliveryPerson { get; set; }
    public DbSet<DeliveryZone> DeliveryZone { get; set; }
    public DbSet<OrderDelivery> OrderDelivery { get; set; }
    public DbSet<OrderItem> OrderItem { get; set; }
    public DbSet<Order> Order { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<Route> Route { get; set; }

    public DomainDbContext(DbContextOptions<DomainDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<DomainEvent>();
    }
}