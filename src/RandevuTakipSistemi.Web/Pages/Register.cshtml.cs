using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RandevuTakipSistemi.Web.Repositories;
using System.ComponentModel.DataAnnotations;

namespace RandevuTakipSistemi.Web.Pages;

[AllowAnonymous]
public class RegisterModel(IRepository<Entities.User> userRepository) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Ad gereklidir.")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; }
    
    [BindProperty]
    [Required(ErrorMessage = "Soyad alanı gereklidir.")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; }
    
    [BindProperty]
    [Required(ErrorMessage = "Email alanı gereklidir.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Şifre alanı gereklidir.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [Display(Name = "Şifre")]
    public string Password { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Şifreyi tekrar girmeniz gereklidir.")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
    [Display(Name = "Şifre (tekrar)")]
    public string ConfirmPassword { get; set; }
    
    public string ErrorMessage { get; set; }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var existingUser = await userRepository.Table.FirstOrDefaultAsync(u => u.Email == Email);
        if (existingUser != null)
        {
            ErrorMessage = "Bu email adresiyle kayıtlı bir kullanıcı zaten mevcut.";
            return Page();
        }
        
        var newUser = new Entities.User
        {
            FirstName = FirstName,
            LastName = LastName,
            Email = Email
        };

        var passwordHasher = new PasswordHasher<Entities.User>();
        var hashedPassword = passwordHasher.HashPassword(newUser, Password);
        newUser.Password = hashedPassword;

        await userRepository.InsertAsync(newUser);
        await userRepository.SaveAllAsync();

        return RedirectToPage("/Login");
    }
}
