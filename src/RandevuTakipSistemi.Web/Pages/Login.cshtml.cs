using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RandevuTakipSistemi.Web.Repositories;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RandevuTakipSistemi.Web.Pages;

[AllowAnonymous]
public class LoginModel(IRepository<Entities.User> userRepository, IConfiguration configuration) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Email alanı gereklidir.")]
    [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Şifre alanı gereklidir.")]
    [Display(Name = "Şifre")]
    public string Password { get; set; }
    
    public string ErrorMessage { get; set; }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await userRepository.Table.FirstOrDefaultAsync(u => u.Email == Email);
        if (user == null)
        {
            ErrorMessage = "Geçersiz email veya şifre.";
            return Page();
        }
        
        var passwordHasher = new PasswordHasher<Entities.User>();
        var passwordVerificationResult = passwordHasher.VerifyHashedPassword(user, user.Password, Password);
        if (passwordVerificationResult != PasswordVerificationResult.Success)
        {
            ErrorMessage = "Geçersiz email veya şifre.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Role, "User"),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTime.UtcNow.AddMinutes(int.Parse(configuration["JwtSettings:ExpiryMinutes"] ?? "30"))
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties);

        return RedirectToPage("/Index");
    }
    
    private string GenerateJwtToken(Entities.User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"] ?? string.Empty));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Role, "User"),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(configuration["JwtSettings:ExpiryMinutes"] ?? "30")),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
