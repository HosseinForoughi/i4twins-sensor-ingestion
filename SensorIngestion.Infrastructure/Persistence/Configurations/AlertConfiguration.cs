using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Infrastructure.Persistence.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable(nameof(Alert));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.RuleId).IsRequired();
        builder.Property(x => x.DeviceId).IsRequired();
        builder.Property(x => x.Metric).IsRequired();
        builder.Property(x => x.StartTs).IsRequired();
        builder.Property(x => x.EndTs).IsRequired();
        builder.Property(x => x.PeakValue).IsRequired();
        builder.Property(x => x.CreationDateTime).IsRequired();

        builder.Property(x => x.DbEntryDateTime)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.HasIndex(x => new { x.RuleId, x.DeviceId, x.Metric, x.StartTs })
            .IsUnique()
            .HasDatabaseName("IX_Alert_NaturalKey");

        builder.Ignore(x => x.NaturalKey);
    }
}