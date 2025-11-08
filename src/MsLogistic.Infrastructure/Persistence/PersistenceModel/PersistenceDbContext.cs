using Microsoft.EntityFrameworkCore;
using MsLogistic.Infrastructure.Persistence.PersistenceModel.EFCoreEntities;

namespace MsLogistic.Infrastructure.Persistence.PersistenceModel;

internal class PersistenceDbContext : DbContext
{
    public DbSet<CustomerPersistenceModel> Customer { get; set; }
    public DbSet<DeliveryPersonPersistenceModel> DeliveryPerson { get; set; }
    public DbSet<DeliveryZonePersistenceModel> DeliveryZone { get; set; }
    public DbSet<ProductPersistenceModel> Product { get; set; }
    public DbSet<OrderPersistenceModel> Order { get; set; }
    public DbSet<OrderItemPersistenceModel> OrderItem { get; set; }
    public DbSet<OrderDeliveryPersistenceModel> OrderDelivery { get; set; }
    public DbSet<RoutePersistenceModel> Route { get; set; }
    
    public PersistenceDbContext(DbContextOptions<PersistenceDbContext> options) : base(options)
    {

    }
    
    public void Migrate()
    {
        Database.Migrate();
    }
}