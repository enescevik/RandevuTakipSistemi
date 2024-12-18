using RandevuTakipSistemi.Web.Enums;

namespace RandevuTakipSistemi.Web.Entities;

public class Notification
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public string Message { get; set; }
    public DateTime SentAt { get; set; }
    public NotificationStatus Status { get; set; }
    public int FailedCount { get; set; }
    public string LastError  { get; set; }
    public Appointment Appointment { get; set; }
}
