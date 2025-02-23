using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ContaBle.Data;
using ContaBle.Models;
using ContaBle.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Telegram.Bot;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// ?? Configurar cliente de Telegram Bot
string botToken = builder.Configuration["TelegramBot:Token"]
    ?? throw new InvalidOperationException("Telegram bot token is not configured.");

// Agregar el servicio de Telegram como HostedService (NO como Singleton)
builder.Services.AddHostedService<TelegramBotService>();

// ?? Registrar el servicio del bot como un servicio en segundo plano (Long Polling)
builder.Services.AddHostedService<TelegramBotService>();
// ?? Configurar Entity Framework Core con Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ?? Configurar autenticación con cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ áéíóúÁÉÍÓÚñÑ";
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


// ?? Configurar servicios de autorización
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

// ?? Configurar servicio de Email
builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailSender, EmailSender>();



var app = builder.Build();

// ?? Obtener Logger y registrar mensaje cuando la aplicación inicie
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("?? El bot de Telegram se ha iniciado correctamente.");

// ?? Configuración del pipeline de la aplicación
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ?? Asegurar que los controladores están configurados correctamente
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
