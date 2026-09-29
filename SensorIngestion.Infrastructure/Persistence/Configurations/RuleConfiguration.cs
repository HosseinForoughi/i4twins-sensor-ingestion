using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Infrastructure.Persistence.Configurations;

public class RuleConfiguration : IEntityTypeConfiguration<Rule>
{
    public void Configure(EntityTypeBuilder<Rule> builder)
    {
        builder.ToTable(nameof(Rule));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.RuleId).IsRequired();
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Enabled).IsRequired();
        builder.Property(x => x.Metric).IsRequired();
        builder.Property(x => x.DeviceId).IsRequired(false);

        builder.Property(x => x.Operator).IsRequired().HasConversion<byte>();

        builder.Property(x => x.Threshold).IsRequired(false);

        builder.Property(x => x.MinValue).IsRequired(false);
        builder.Property(x => x.MaxValue).IsRequired(false);

        builder.Property(x => x.DurationSeconds).IsRequired(false);
        builder.Property(x => x.CreationDateTime).IsRequired();

        builder.Property(x => x.DbEntryDateTime)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.HasIndex(x => x.RuleId)
            .IsUnique()
            .HasDatabaseName("IX_Rule_RuleId");

        builder.Ignore(x => x.IsStateful);
    }
}