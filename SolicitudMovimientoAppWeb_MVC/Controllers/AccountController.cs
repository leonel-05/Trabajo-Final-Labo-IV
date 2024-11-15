using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolicitudMovimientoAppWeb_MVC.Models;

namespace SolicitudMovimientoAppWeb_MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        //Configuracion del controller para tener acceso
        public AccountController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;//a la base de datos lectura/escritura
            _signInManager = signInManager;//gestionar usuarios del sistema
            _roleManager = roleManager;//Gestiona los Roles de los usuarios
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Generador"))
                    return RedirectToAction("Index", "Generador");
                else if (roles.Contains("Delegador"))
                    return RedirectToAction("Index", "Delegador");
                else if (roles.Contains("Chofer"))
                    return RedirectToAction("MisMovimientos", "Chofer");

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Login inválido.");
            return View();
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        //POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string nombre, string email, string password, string role)
        {
            var user = new Usuario { UserName = email, Email = email, Nombre = nombre };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role); // Asignar el rol
                return RedirectToAction("Login", "Account"); // Redirigir al login
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // GET: Listar Usuarios
        [Authorize(Roles = "Delegador")]
        public async Task<IActionResult> ListUsers(int? page, int pageSize = 10)
        {
            var users = await _userManager.Users.ToListAsync();
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            var userRoles = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                // Obtener el rol de cada usuario individualmente para evitar conflictos de concurrencia
                var userRole = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.Nombre,
                    Email = user.Email,
                    CurrentRole = userRole.FirstOrDefault() ?? "Sin rol"
                });
            }

            // Paginación manual
            int pageNumber = page ?? 1;
            int totalItems = userRoles.Count;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = pageNumber;

            // Obtener solo los elementos de la página actual
            var paginatedList = userRoles.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Roles = roles; // Pasamos los roles disponibles a la vista
            return View(paginatedList);
        }


        // POST: Cambiar Rol del Usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUserRole(int userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            if (!await _roleManager.RoleExistsAsync(newRole))
            {
                return BadRequest("El rol especificado no existe");
            }

            var result = await _userManager.AddToRoleAsync(user, newRole);
            if (result.Succeeded)
            {
                return RedirectToAction("ListUsers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("Error");
        }

        // GET: Account/MisTareas
        [Authorize]
        public async Task<IActionResult> MisTareas()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Delegador"))
            {
                return RedirectToAction("Index", "Delegador");
            }
            else if (roles.Contains("Generador"))
            {
                return RedirectToAction("Index", "Generador");
            }
            else if (roles.Contains("Chofer"))
            {
                return RedirectToAction("MisMovimientos", "Chofer");
            }

            return RedirectToAction("Index", "Home"); // Redirigir a una vista predeterminada si no se encuentra el rol
        }
    }

}


