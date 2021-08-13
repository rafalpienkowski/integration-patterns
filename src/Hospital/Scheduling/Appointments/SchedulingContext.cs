using Microsoft.EntityFrameworkCore;

namespace Scheduling.Appointments
{
    public class CustomersContext : DbContext
    {
        private const string Schema = "scheduling";
        public DbSet<Customer> Customers { get; set; }
        
        
        public CustomersContext(DbContextOptions<CustomersContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);
            base.OnModelCreating(modelBuilder);
        }
    }
}