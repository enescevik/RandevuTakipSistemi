using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandevuTakipSistemi.Web.Entities;

namespace RandevuTakipSistemi.Web.Repositories.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.SentAt)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.FailedCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.LastError)
            .HasMaxLength(1000);

        builder
            .HasOne(x => x.Appointment)
            .WithMany(x => x.Notifications)
            .HasForeignKey(x => x.AppointmentId);
    }
}
