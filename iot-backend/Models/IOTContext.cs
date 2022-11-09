using Microsoft.EntityFrameworkCore;

namespace iot_backend.Models;

public class IOTContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=DESKTOP-LCSVRN2;Database=IOTHubSensorDB;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False;");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSensor>().HasKey(uS => new { uS.UserId, uS.SensorId });
        
        modelBuilder.Entity<User>()
            .Property(u => u.UserId)
            .ValueGeneratedOnAdd();
        
        modelBuilder.Entity<SensorData>()
            .Property(sD => sD.SensorDataId)
            .ValueGeneratedOnAdd();
        
        modelBuilder.Entity<SensorData>()
            .HasOne(sD => sD.Sensor)
            .WithMany(s => s.Datas)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<UserSensor>()
            .HasOne(uS => uS.User)
            .WithMany(u => u.Sensors)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<UserSensor>()
            .HasOne(uS => uS.Sensor)
            .WithMany(s => s.Users)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<SensorData> SensorDatas { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserSensor> UserSensors { get; set; }
}