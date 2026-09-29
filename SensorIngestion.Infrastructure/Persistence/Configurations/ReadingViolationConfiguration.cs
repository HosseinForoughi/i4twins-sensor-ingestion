using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorIngestion.Domain.Entities;

namespace SensorIngestion.Infrastructure.Persistence.Configurations;

public class ReadingViolationConfiguration : IEntityTypeConfiguration<ReadingViolation>
{
    public void Configure(EntityTypeBuilder<ReadingViolation> builder)
    {
        builder.ToTable(nameof(ReadingViolation));

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.ReadingId).IsRequired();
        builder.Property(x => x.RuleId).IsRequired();
        builder.Property(x => x.Reason).IsRequired();
        builder.Property(x => x.CreationDateTime).IsRequired();

        builder.Property(x => x.DbEntryDateTime)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.HasIndex(x => new { x.ReadingId, x.RuleId })
            .IsUnique()
            .HasDatabaseName("IX_ReadingViolation_Reading_Rule");

        builder.HasIndex(x => x.RuleId)
            .HasDatabaseName("IX_ReadingViolation_RuleId");
    }
}
