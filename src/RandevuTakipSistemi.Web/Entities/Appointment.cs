using RandevuTakipSistemi.Web.Enums;

namespace RandevuTakipSistemi.Web.Entities;

public class Appointment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AppointmentStatus Status { get; set; }
    
    public User User { get; set; }
    public ICollection<Notification> Notifications { get; set; }
}
