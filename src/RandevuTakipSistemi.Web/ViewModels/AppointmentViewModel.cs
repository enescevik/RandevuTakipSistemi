using RandevuTakipSistemi.Web.Enums;

namespace RandevuTakipSistemi.Web.ViewModels;

public record AppointmentViewModel
{
    public int Id { get; set; }
    public string Title { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public AppointmentStatus Status { get; init; }
    
    public bool AddReminder { get; init; }
    public string NotificationMessage { get; init; }
    public DateTime? NotificationDate { get; init; }
    public NotificationStatus NotificationStatus { get; init; }
    public int FailedCount { get; init; }
    public string LastError { get; init; }
}
