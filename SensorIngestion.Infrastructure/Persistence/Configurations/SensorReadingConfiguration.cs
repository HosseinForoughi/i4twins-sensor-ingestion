using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Infrastructure.Persistence.Configurations;

public class SensorReadingConfiguration : IEntityTypeConfiguration<SensorReading>
{
    public void Configure(EntityTypeBuilder<SensorReading> builder)
    {
        builder.ToTable(nameof(SensorReading));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.DeviceId).IsRequired();
        builder.Property(x => x.Metric).IsRequired();
        builder.Property(x => x.Timestamp).IsRequired();
        builder.Property(x => x.Sequence).IsRequired();

        builder.Property(x => x.Value).IsRequired();

        builder.Property(x => x.Classification)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.CreationDateTime).IsRequired();

        builder.Property(x => x.DbEntryDateTime)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.HasIndex(x => new { x.DeviceId, x.Metric, x.Timestamp, x.Sequence })
            .IsUnique()
            .HasDatabaseName("IX_Reading_NaturalKey");

        builder.Ignore(x => x.NaturalKey);

        builder.HasMany(x => x.Violations)
            .WithOne(x => x.Reading)
            .HasForeignKey(x => x.ReadingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Violations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}