using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SolicitudMovimientoAppWeb_MVC.Models;

namespace SolicitudMovimientoAppWeb_MVC.Controllers
{
    [Authorize(Roles = "Delegador")]
    public class DelegadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        //Configuracion del controller para tener acceso
        public DelegadorController(ApplicationDbContext context, UserManager<Usuario> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _context = context;//a la base de datos lectura/escritura
            _userManager = userManager;//gestionar usuarios del sistema
            _roleManager = roleManager;//enviar correos electronicos
        }

        // GET: Delegador/Index
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            var movimientos = await _context.Movimientos
                .Where(m => m.EstadoId == 1)//Filtra los movimiento con estado pendiente
                .Include(m => m.LugarSalida)
                .Include(m => m.LugarDestino)
                .Include(m => m.Estado)
                .ToListAsync();//Lista filtrada por el WHERE

            var totalItems = await _context.Movimientos.CountAsync(m => m.EstadoId == 1);
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = page;

            return View(movimientos);
        }

        // GET: Delegador/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var movimiento = await _context.Movimientos
                .Include(m => m.LugarSalida)
                .Include(m => m.LugarDestino)
                .FirstOrDefaultAsync(m => m.Id == id);
                if (movimiento == null)
                {
                    return NotFound();
                }

                var choferes = await _userManager.GetUsersInRoleAsync("Chofer");
                var estados = _context.Estados.ToList();

                if (choferes == null || estados == null)
                {
                    ModelState.AddModelError("", "Error al cargar los datos de choferes o estados.");
                    return View();
                }

                ViewBag.Choferes = new SelectList(choferes, "Id", "UserName");//Trae los usuarios con rol chofer por el Id muestra username
                ViewBag.Estados = new SelectList(estados, "Id", "Descripcion");//Trae los estados por el Id muestra descripción

                return View(movimiento);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al cargar los datos del movimiento: {ex.Message}");
                return View();
            }
        }

        // POST: Delegador/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movimiento movimiento)
        {
            if (id != movimiento.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var movimientoExistente = await _context.Movimientos.FindAsync(id);
                    if (movimientoExistente == null)
                    {
                        return NotFound();
                    }

                    // Validar que el movimiento tenga un ChoferId asignado si el estado es "Atendido"
                    if (movimiento.EstadoId == 2 && movimiento.ChoferId == null)
                    {
                        ModelState.AddModelError("", "Debe asignar un chofer antes de marcar el movimiento como 'Atendido'.");
                        ViewBag.Choferes = await ObtenerChoferesAsync();
                        ViewBag.Estados = _context.Estados.ToList();
                        return View(movimiento);
                    }

                    // Asigna los valores solo si han cambiado para evitar inconsistencias
                    if (movimientoExistente.ChoferId != movimiento.ChoferId)
                        movimientoExistente.ChoferId = movimiento.ChoferId;

                    if (movimientoExistente.EstadoId != movimiento.EstadoId)
                        movimientoExistente.EstadoId = movimiento.EstadoId;

                    // Asegúrate de no modificar FechaSolicitud a menos que sea necesario
                    if (movimientoExistente.FechaSolicitud == DateTime.MinValue)
                        movimientoExistente.FechaSolicitud = DateTime.Now;

                    _context.Update(movimientoExistente);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Error al actualizar el movimiento: {ex.Message}");
                }
            }

            ViewBag.Choferes = await ObtenerChoferesAsync();
            ViewBag.Estados = _context.Estados.ToList();
            return View(movimiento);
        }


        private async Task<List<Usuario>> ObtenerChoferesAsync()
        {
            var choferes = new List<Usuario>();
            var usuarios = _userManager.Users.ToList();

            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                if (roles.Contains("Chofer"))
                {
                    choferes.Add(usuario);
                }
            }
            return choferes;
        }


        private bool MovimientoExists(int id)
        {
            return _context.Movimientos.Any(e => e.Id == id);
        }
    }
}


