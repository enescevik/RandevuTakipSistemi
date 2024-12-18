using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RandevuTakipSistemi.Web.Entities;
using RandevuTakipSistemi.Web.Enums;
using RandevuTakipSistemi.Web.Repositories;
using RandevuTakipSistemi.Web.ViewModels;
using System.Security.Claims;

namespace RandevuTakipSistemi.Web.Pages;

public class IndexModel(
    IRepository<Appointment> appointmentRepository,
    IRepository<Notification> notificationRepository,
    IAntiforgery antiforgery) : PageModel
{
    [BindProperty]
    public AppointmentViewModel Appointment { get; set; }
    
    public List<AppointmentViewModel> Appointments { get; set; } = new();
    
    public string RequestVerificationToken => antiforgery.GetAndStoreTokens(HttpContext).RequestToken;

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.User.Identity is not { IsAuthenticated: true })
            return RedirectToPage("/Login");

        Appointments = await appointmentRepository.Table
            .AsNoTracking()
            .Select(x => new AppointmentViewModel
            {
                Id = x.Id,
                Title = x.Title,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status,
                AddReminder = x.Notifications.Any(),
                NotificationMessage = x.Notifications.Any()
                    ? x.Notifications.FirstOrDefault().Message
                : null,
                NotificationDate = x.Notifications.Any()
                    ? x.Notifications.FirstOrDefault().SentAt
                    : null
            })
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAddEventAsync()
    {
        if (!ModelState.IsValid)
        {
            TempData["ModalState"] = "show";
            return Page();
        }
        
        if (Appointment.AddReminder
            && (string.IsNullOrWhiteSpace(Appointment.NotificationMessage)
                || !Appointment.NotificationDate.HasValue))
        {
            TempData["ModalState"] = "show";
            TempData["ErrorMessage"] = "Bildirim mesajı ve tarihi seçmelisiniz!";
            return Page();
        }
        
        var updatedEvent = await appointmentRepository.Table.FirstOrDefaultAsync(x => x.Id == Appointment.Id);
        if (updatedEvent != null)
        {
            updatedEvent.Title = Appointment.Title;
            updatedEvent.StartDate = Appointment.StartDate;
            updatedEvent.EndDate = Appointment.EndDate;
            await appointmentRepository.SaveAllAsync();

            var existingNotifications = await notificationRepository.Table
                .Where(x => x.AppointmentId == Appointment.Id && x.Status == NotificationStatus.Waiting)
                .ToListAsync();
            notificationRepository.DeleteRange(existingNotifications);
            await notificationRepository.SaveAllAsync();
            
            await AddNotificationAsync(Appointment);

            TempData["ModalState"] = string.Empty;
            return RedirectToPage("/Index");
        }
        
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return RedirectToPage("/Login");
        
        var newAppointment = new Appointment
        {
            Title = Appointment.Title,
            StartDate = Appointment.StartDate,
            EndDate = Appointment.EndDate,
            UserId = int.Parse(userIdClaim.Value),
            Status = AppointmentStatus.Pending,
        };
        await appointmentRepository.InsertAsync(newAppointment);
        await appointmentRepository.SaveAllAsync();
        
        Appointment.Id = newAppointment.Id;
        
        await AddNotificationAsync(Appointment);

        TempData["ModalState"] = string.Empty;
        return RedirectToPage("/Index");
    }

    public async Task AddNotificationAsync(AppointmentViewModel appointment)
    {
        if (!Appointment.AddReminder || !Appointment.NotificationDate.HasValue) return;

        await notificationRepository.InsertAsync(new()
        {
            AppointmentId = Appointment.Id,
            Message = Appointment.NotificationMessage,
            SentAt = Appointment.NotificationDate.Value,
            Status = NotificationStatus.Waiting
        });
        await notificationRepository.SaveAllAsync();
    }

    public async Task<IActionResult> OnPostDeleteEventAsync()
    {
        var existingEvent = await appointmentRepository.Table.FirstOrDefaultAsync(x => x.Id == Appointment.Id);
        if (existingEvent == null)
        {
            TempData["ModalState"] = "show";
            TempData["ErrorMessage"] = "Randevu bulunamadı.";
            return Page();
        }

        appointmentRepository.Delete(existingEvent);
        await appointmentRepository.SaveAllAsync();

        TempData["ModalState"] = string.Empty;
        return RedirectToPage("/Index");
    }
}
