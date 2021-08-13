using Microsoft.EntityFrameworkCore;

namespace Scheduling.Appointments
{
    public class SchedulingContext : DbContext
    {
        private const string Schema = "scheduling";
        public DbSet<Patient> Customers { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        
        public SchedulingContext(DbContextOptions<SchedulingContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);
            base.OnModelCreating(modelBuilder);
        }
    }
}