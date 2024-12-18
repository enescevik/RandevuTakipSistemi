using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RandevuTakipSistemi.Web.Entities;
using RandevuTakipSistemi.Web.Repositories;
using RandevuTakipSistemi.Web.ViewModels;

namespace RandevuTakipSistemi.Web.Pages;

public class NotificationsModel(IRepository<Notification> notificationRepository) : PageModel
{
    public List<AppointmentViewModel> Appointments { get; set; }
    
    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.User.Identity is not { IsAuthenticated: true })
            return RedirectToPage("/Login");

        Appointments = await notificationRepository.Table
            .Include(n => n.Appointment)
            .AsNoTracking()
            .OrderBy(a => a.SentAt)
            .Select(x => new AppointmentViewModel
            {
                Id = x.Appointment.Id,
                StartDate = x.Appointment.StartDate,
                EndDate = x.Appointment.EndDate,
                Title = x.Appointment.Title,
                Status = x.Appointment.Status,
                AddReminder = true,
                NotificationMessage = x.Message,
                NotificationDate = x.SentAt,
                NotificationStatus = x.Status,
                FailedCount = x.FailedCount,
                LastError = x.LastError
            })
            .ToListAsync();

        return Page();
    }
}
