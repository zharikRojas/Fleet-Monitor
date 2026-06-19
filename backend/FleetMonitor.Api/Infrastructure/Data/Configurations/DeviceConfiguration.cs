using FleetMonitor.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FleetMonitor.Api.Infrastructure.Data.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.MaskedId).HasMaxLength(32).IsRequired();
        builder.Property(d => d.Name).HasMaxLength(128).IsRequired();
        builder.HasIndex(d => d.MaskedId).IsUnique();
        builder.Property(d => d.UpdatedAt).IsRequired();
    }
}
