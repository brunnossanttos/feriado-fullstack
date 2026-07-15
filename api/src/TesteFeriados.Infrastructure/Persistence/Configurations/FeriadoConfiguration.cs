using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TesteFeriados.Domain.Entities;

namespace TesteFeriados.Infrastructure.Persistence.Configurations;

public class FeriadoConfiguration : IEntityTypeConfiguration<Feriado>
{
    public void Configure(EntityTypeBuilder<Feriado> builder)
    {
        builder.ToTable("Feriados");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(f => f.Title).IsUnique();

        builder.Property(f => f.Date).HasMaxLength(10);
        builder.Property(f => f.Type).HasMaxLength(50);
        builder.Property(f => f.StartTime).HasMaxLength(10);
        builder.Property(f => f.EndTime).HasMaxLength(10);

        builder.Property(f => f.Description);
        builder.Property(f => f.Legislation);

        var comparer = new ValueComparer<Dictionary<string, string>>(
            (a, b) => JsonSerializer.Serialize(a, JsonOptions) == JsonSerializer.Serialize(b, JsonOptions),
            v => JsonSerializer.Serialize(v, JsonOptions).GetHashCode(),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(
                     JsonSerializer.Serialize(v, JsonOptions), JsonOptions) ?? new Dictionary<string, string>());

        builder.Property(f => f.VariableDates)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, JsonOptions) ?? new Dictionary<string, string>())
            .Metadata.SetValueComparer(comparer);
    }

    private static readonly JsonSerializerOptions JsonOptions = new();
}
