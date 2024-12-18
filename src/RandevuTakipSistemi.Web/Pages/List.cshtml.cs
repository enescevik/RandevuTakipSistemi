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

public class ListModel(IRepository<Appointment> appointmentRepository, IAntiforgery antiforgery) : PageModel
{
    [BindProperty]
    public AppointmentViewModel Appointment { get; set; }
    
    public List<Appointment> Appointments { get; set; } = new();
    
    public string RequestVerificationToken => antiforgery.GetAndStoreTokens(HttpContext).RequestToken;

    public async Task<IActionResult> OnGetAsync()
    {
        if (HttpContext.User.Identity is not { IsAuthenticated: true })
            return RedirectToPage("/Login");

        Appointments = await appointmentRepository.Table
            .AsNoTracking()
            .OrderBy(a => a.StartDate)
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
        
        var updatedEvent = await appointmentRepository.Table.FirstOrDefaultAsync(x => x.Id == Appointment.Id);
        if (updatedEvent != null)
        {
            updatedEvent.Title = Appointment.Title;
            updatedEvent.StartDate = Appointment.StartDate;
            updatedEvent.EndDate = Appointment.EndDate;

            await appointmentRepository.SaveAllAsync();

            TempData["ModalState"] = string.Empty;
            return RedirectToPage("/List");
        }
        
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return RedirectToPage("/Login");
        
        var newAppointment = new Appointment
        {
            Title = Appointment.Title,
            StartDate = Appointment.StartDate,
            EndDate = Appointment.EndDate,
            UserId = int.Parse(userIdClaim.Value),
            Status = AppointmentStatus.Pending
        };
        await appointmentRepository.InsertAsync(newAppointment);
        await appointmentRepository.SaveAllAsync();

        TempData["ModalState"] = string.Empty;
        return RedirectToPage("/List");
    }

    public async Task<IActionResult> OnPostDeleteEventAsync(int id)
    {
        var existingEvent = await appointmentRepository.Table.FirstOrDefaultAsync(x => x.Id == id);
        if (existingEvent == null)
        {
            TempData["ErrorMessage"] = "Randevu bulunamadı.";
            return Page();
        }

        appointmentRepository.Delete(existingEvent);
        await appointmentRepository.SaveAllAsync();

        return RedirectToPage("/List");
    }
}
