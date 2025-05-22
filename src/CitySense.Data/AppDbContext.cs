using Microsoft.EntityFrameworkCore;
using CitySense.Data.Models; 

namespace CitySense.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<SensorDataPoint> SensorDataPoints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("postgis");

           
            modelBuilder.Entity<SensorDataPoint>()
                .HasIndex(sdp => sdp.Timestamp);

           
        }
    }
}