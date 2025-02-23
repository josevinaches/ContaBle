using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ContaBle.Models;
using ContaBle.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;
using ContaBle.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContaBle.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ApplicationDbContext context,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            var companies = _context.Companies.ToList();
            ViewBag.Companies = new SelectList(companies, "Id", "Name");
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Buscar la compañía seleccionada en la base de datos
                var selectedCompany = await _context.Companies.FindAsync(model.CompanyId);
                if (selectedCompany == null)
                {
                    ModelState.AddModelError("CompanyId", "La compañía seleccionada no es válida.");
                    var companies = _context.Companies.ToList();
                    ViewBag.Companies = new SelectList(companies, "Id", "Name");
                    return View(model);
                }

                // Crear usuario con la compañía asignada
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    CompanyId = model.CompanyId // Se asigna la compañía seleccionada
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email, "📩 ¡Confirma tu cuenta en ContaBle!",
                        $@"
                    <h2>¡Bienvenido a ContaBle! 🎉</h2>
                    <p>Gracias por registrarte. Antes de acceder, necesitamos que confirmes tu cuenta.</p>
                    <p>Haz clic en el siguiente botón para confirmar:</p>
                    <p><a href='{callbackUrl}' style='background-color:#28a745;color:white;padding:10px 20px;text-decoration:none;border-radius:5px;'>✅ Confirmar Cuenta</a></p>
                    <p>Si no fuiste tú quien intentó registrarse, ignora este mensaje.</p>
                    <p>Saludos,<br>📌 El equipo de ContaBle</p>");

                    return Redirect(callbackUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var companiesList = _context.Companies.ToList();
            ViewBag.Companies = new SelectList(companiesList, "Id", "Name");
            return View(model);
        }




        [HttpGet]
        public async Task<IActionResult> TelegramVerification(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // En la vista se mostrará el código de verificación, en este ejemplo usamos el user.Id.
            // Instrucciones: "Envía el comando /verificar <código> a nuestro bot @asantamartaBot"
            ViewBag.VerificationCode = user.Id;
            ViewBag.Email = user.Email; // Pasamos el email a la vista
            return View();
        }


        

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                user.EmailConfirmed = true;  // 🔹 Asegurar que se marca como confirmado
                await _userManager.UpdateAsync(user);  // 🔹 Guardar el cambio
                _logger.LogInformation($"[DEBUG] Confirmación de correo exitosa para el usuario: {user.UserName}. EmailConfirmed: {user.EmailConfirmed}");
                // Guardar el código de verificación de Telegram en ViewBag
                ViewBag.VerificationCode = user.Id;
                ViewBag.Email = user.Email;

                return View("TelegramVerification"); // Asegura que la vista está en Views/Account
            }

            return View("Error");
        }



        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Intentar encontrar el usuario por UserName
                var user = await _userManager.FindByNameAsync(model.UserName);

                // Si no se encuentra por UserName, intentar buscar por Email
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(model.UserName);
                }

                if (user == null)
                {
                    _logger.LogWarning($"[DEBUG] No se encontró el usuario con UserName o Email: {model.UserName}");
                    ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                    return View(model);
                }

                _logger.LogInformation($"[DEBUG] Usuario encontrado: {user.UserName} - EmailConfirmed: {user.EmailConfirmed}");

                if (!user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Debes confirmar tu correo antes de iniciar sesión.");
                    return View(model);
                }

                _logger.LogInformation($"[DEBUG] Usuario {user.UserName} - TelegramVerified: {user.TelegramVerified}");
                if (!user.TelegramVerified)
                {
                    ModelState.AddModelError(string.Empty, "Debes verificar tu cuenta en Telegram antes de iniciar sesión.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("UserProfile", "Account");
                }

                ModelState.AddModelError(string.Empty, "Intento de inicio de sesión inválido.");
            }

            return View(model);
        }




        [HttpGet]
        [AutoValidateAntiforgeryToken]
        [Authorize]
        public async Task<IActionResult> UserProfile(string searchTerm, int pageNumber = 1, int pageSize = 10, string sortOrder = "FirstName")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var personsQuery = _context.Persons.Where(p => p.UserId == user.Id);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                personsQuery = personsQuery.Where(p => p.FirstName.Contains(searchTerm) 
                || p.LastName.Contains(searchTerm) 
                || p.Dni.Contains(searchTerm)
                || p.Email.Contains(searchTerm));
                ViewData["searchTerm"] = searchTerm;
            }

            switch (sortOrder)
            {
                case "FirstName":
                    personsQuery = personsQuery.OrderBy(p => p.FirstName);
                    break;
                case "LastName":
                    personsQuery = personsQuery.OrderBy(p => p.LastName);
                    break;
                case "Email":
                    personsQuery = personsQuery.OrderBy(p => p.Email);
                    break;
                default:
                    personsQuery = personsQuery.OrderBy(p => p.FirstName);
                    break;
            }

            ViewData["sortOrder"] = sortOrder;

            var totalRecords = await personsQuery.CountAsync();
            var persons = await personsQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new UserProfileViewModel
            {
                Email = user.Email,
                UserName = user.UserName,
                NormalizedUserName = user.NormalizedUserName,
                Persons = persons, // Pasar la lista de personas al modelo de vista
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Por razones de seguridad, no revelamos que el usuario no existe o no está confirmado
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                // Generar el token para restablecer contraseña
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Codificar el token para que sea seguro en la URL
                var encodedToken = System.Net.WebUtility.UrlEncode(token);

                // Crear el enlace para restablecer contraseña
                var callbackUrl = Url.Action("ResetPassword", "Account",
                    new { token = encodedToken, email = model.Email }, protocol: HttpContext.Request.Scheme);

                // Enviar el correo electrónico con el enlace
                await _emailSender.SendEmailAsync(model.Email, "Restablecer Contraseña",
                    $"Por favor restablece tu contraseña haciendo clic <a href='{callbackUrl}'>aquí</a>.");

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }


        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest("Token o correo inválido.");
            }

            // Decodifica el token
            var decodedToken = System.Net.WebUtility.UrlDecode(token);

            var model = new ResetPasswordViewModel
            {
                Token = decodedToken,
                Email = email
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            // Decodifica el token antes de validar
            var decodedToken = System.Net.WebUtility.UrlDecode(model.Token);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }



        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        [HttpGet]
        [AutoValidateAntiforgeryToken]
        [Authorize]
        public IActionResult Welcome()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePerson(CreatePersonViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingDni = await _context.Persons.AnyAsync(p => p.Dni == model.Dni);
                var existingNumeroSocio = await _context.Persons.AnyAsync(p => p.NumeroSocio == model.NumeroSocio);

                if (existingDni)
                {
                    ModelState.AddModelError("Dni", "El DNI ya está en uso.");
                }

                if (existingNumeroSocio)
                {
                    ModelState.AddModelError("NumeroSocio", "El Número de Socio ya está en uso.");
                }

                if (!existingDni && !existingNumeroSocio)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    var person = new Person
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        Dni = model.Dni,
                        FechaNacimiento = model.FechaNacimiento,
                        Nota = model.Nota,
                        NumeroSocio = model.NumeroSocio,
                        CuotaSocio = model.CuotaSocio,
                        CuotaFester = model.CuotaFester,
                        UserId = user.Id
                    };

                    _context.Persons.Add(person);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("UserProfile");
                }
            }

            return View(model);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditPerson(EditPersonViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingDni = await _context.Persons.AnyAsync(p => p.Dni == model.Dni && p.Id != model.Id);
                var existingNumeroSocio = await _context.Persons.AnyAsync(p => p.NumeroSocio == model.NumeroSocio && p.Id != model.Id);

                if (existingDni)
                {
                    ModelState.AddModelError("Dni", "El DNI ya está en uso.");
                }

                if (existingNumeroSocio)
                {
                    ModelState.AddModelError("NumeroSocio", "El Número de Socio ya está en uso.");
                }

                if (!existingDni && !existingNumeroSocio)
                {
                    var person = await _context.Persons.FindAsync(model.Id);
                    if (person == null)
                    {
                        return NotFound();
                    }

                    person.FirstName = model.FirstName;
                    person.LastName = model.LastName;
                    person.Email = model.Email;
                    person.PhoneNumber = model.PhoneNumber;
                    person.Dni = model.Dni;
                    person.FechaNacimiento = model.FechaNacimiento;
                    person.Nota = model.Nota;
                    person.NumeroSocio = model.NumeroSocio;
                    person.CuotaSocio = model.CuotaSocio;
                    person.CuotaFester = model.CuotaFester;

                    _context.Persons.Update(person);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("UserProfile");
                }
            }

            return View(model);
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePerson(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person == null || person.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserProfile");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> PersonDetails(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person == null || person.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            var model = new PersonDetailsViewModel
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email,
                PhoneNumber = person.PhoneNumber,
                Dni = person.Dni,
                FechaNacimiento = person.FechaNacimiento,
                Nota = person.Nota,
                NumeroSocio = person.NumeroSocio,
                CuotaSocio = person.CuotaSocio,
                CuotaFester = person.CuotaFester

            };

            return View(model);
        }





        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditPerson(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person == null || person.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            var model = new EditPersonViewModel
            {
                Id = person.Id,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email,
                PhoneNumber = person.PhoneNumber,
                Dni = person.Dni,
                FechaNacimiento = person.FechaNacimiento,
                Nota = person.Nota,
                NumeroSocio = person.NumeroSocio,
                CuotaSocio = person.CuotaSocio,
                CuotaFester = person.CuotaFester
            };

            return View(model);
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SendDetailsEmail(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person == null || person.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(person.UserId);
            var currentYear = DateTime.Now.Year;
            var totalAbonado = person.CuotaSocio + person.CuotaFester;
            var emailContent = $@"
        <h1>Asociación Santa Marta</h1>   
        <h3>Detalles de la Persona</h3>
        <p><strong>Nombre:</strong> {person.FirstName}</p>
        <p><strong>Apellido:</strong> {person.LastName}</p>
        <p><strong>Correo Electrónico:</strong> {person.Email}</p>
        <p><strong>Número de Teléfono:</strong> {person.PhoneNumber}</p>
        <p><strong>DNI:</strong> {person.Dni}</p>
        <p><strong>Fecha de Nacimiento:</strong> {person.FechaNacimiento:dd/MM/yyyy}</p>
        <p><strong>Nota:</strong> {person.Nota}</p>
        <p><strong>Número de Socio:</strong> {person.NumeroSocio}</p>
        <p><strong>Cuota Socio:</strong> {person.CuotaSocio:C}</p>
        <p><strong>Cuota Fester:</strong> {person.CuotaFester:C}</p>
        <p><strong>Correo Representante: {person.Nota}</strong> {user.UserName}</p>
        <p><strong>Total Abonado:</strong> <mark>{totalAbonado:C}</mark></p>
        <p>Sus datos se han guardado en la base de datos de la Asociación de Santa Marta. Puede solicitar el borrado de sus datos personales en cualquier momento.</p>
        <p><strong>Año en Curso:</strong> {currentYear}</p>";

            await _emailSender.SendEmailAsync(person.Email, "Detalles de la Persona", emailContent);

            TempData["Message"] = "Los detalles se han enviado por correo electrónico.";
            return RedirectToAction("PersonDetails", new { id = person.Id });
        }
    }
}