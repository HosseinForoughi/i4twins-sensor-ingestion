using Microsoft.EntityFrameworkCore;
using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Infrastructure.Persistence;

public class SensorIngestionDbContext : DbContext
{
    public SensorIngestionDbContext(DbContextOptions<SensorIngestionDbContext> options) : base(options)
    {
    }

    public DbSet<SensorReading> Readings => Set<SensorReading>();

    public DbSet<ReadingViolation> ReadingViolations => Set<ReadingViolation>();

    public DbSet<Alert> Alerts => Set<Alert>();

    public DbSet<Rule> Rules => Set<Rule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SensorIngestionDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}