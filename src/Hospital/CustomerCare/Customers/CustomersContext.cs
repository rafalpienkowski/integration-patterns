using Microsoft.EntityFrameworkCore;

namespace CustomerCare.Customers
{
    public class CustomersContext : DbContext
    {
        private const string Schema = "customercare";
        
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