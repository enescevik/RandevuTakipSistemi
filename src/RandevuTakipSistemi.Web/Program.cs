using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RandevuTakipSistemi.Web.Repositories;
using RandevuTakipSistemi.Web.Services;
using RandevuTakipSistemi.Web.Workers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
var configuration = builder.Configuration;

services.AddRazorPages();

services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidAudience = configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"] ?? string.Empty))
        };
    });

services.AddAuthorization();

var connectionString = configuration.GetConnectionString("ApplicationDbContext");
services.AddDbContext<ApplicationDbContext>(options
    => options.UseNpgsql(connectionString, x => x.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

services.AddScoped(typeof(IRepository<>), typeof(LinqToSqlRepository<>));

services.AddScoped<INotificationService, NotificationService>();
services.AddHostedService<NotificationWorker>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
