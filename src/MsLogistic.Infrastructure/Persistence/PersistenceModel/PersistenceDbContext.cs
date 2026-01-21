using Microsoft.EntityFrameworkCore;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel;

internal class PersistenceDbContext : DbContext
{
    public DbSet<BatchPersistenceModel> Batches { get; set; }
    public DbSet<CustomerPersistenceModel> Customers { get; set; }
    public DbSet<DeliveryZonePersistenceModel> DeliveryZones { get; set; }
    public DbSet<DriverPersistenceModel> Drivers { get; set; }
    public DbSet<OrderDeliveryPersistenceModel> OrderDeliveries { get; set; }
    public DbSet<OrderIncidentPersistenceModel> OrderIncidents { get; set; }
    public DbSet<OrderItemPersistenceModel> OrderItems { get; set; }
    public DbSet<OrderPersistenceModel> Orders { get; set; }
    public DbSet<ProductPersistenceModel> Products { get; set; }
    public DbSet<RoutePersistenceModel> Routes { get; set; }

    public PersistenceDbContext(DbContextOptions<PersistenceDbContext> options) : base(options)
    {
    }
}