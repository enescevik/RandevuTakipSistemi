using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RandevuTakipSistemi.Web.Entities;

namespace RandevuTakipSistemi.Web.Repositories.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(x => x.EndDate)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder
            .HasOne(x => x.User)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.UserId);
    }
}
