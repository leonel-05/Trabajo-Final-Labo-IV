using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolicitudMovimientoAppWeb_MVC.Models;
using SolicitudMovimientoAppWeb_MVC.Services;

namespace SolicitudMovimientoAppWeb_MVC.Controllers
{
    [Authorize(Roles = "Chofer")]
    public class ChoferController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;
        private readonly EmailService _emailService;

        //Configuracion del controller para tener acceso
        public ChoferController(ApplicationDbContext context, UserManager<Usuario> userManager, EmailService emailService)
        {
            _context = context;//a la base de datos lectura/escritura
            _userManager = userManager;//gestionar usuarios del sistema
            _emailService = emailService;//enviar correos electronicos
        }

        // GET: Chofer/Movimientos
        public async Task<IActionResult> MisMovimientos()
        {
            var usuarioId = int.Parse(_userManager.GetUserId(User));

            var movimientos = await _context.Movimientos
                .Include(m => m.LugarSalida)
                .Include(m => m.LugarDestino)
                .Where(m => m.ChoferId == usuarioId && m.EstadoId == 2)//filtra la vista con el chofer y los atendidos pero no realizados.
                .ToListAsync();//la lista filtrada por el WHERE

            return View(movimientos);
        }

        // GET: Chofer/ActualizarEstado/5
        public async Task<IActionResult> ActualizarEstado(int id)
        {
            var movimiento = await _context.Movimientos
                .Include(m => m.LugarSalida)
                .Include(m => m.LugarDestino)
                .Include(m => m.Generador) // Para obtener la información del Generador
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movimiento == null || movimiento.ChoferId != int.Parse(_userManager.GetUserId(User)))
            {
                return NotFound();
            }

            ViewBag.Estados = _context.Estados.ToList();
            return View(movimiento);
        }

        // POST: Chofer/ActualizarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstado(int id, Movimiento movimiento, IFormFile foto)
        {
            if (id != movimiento.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var movimientoExistente = await _context.Movimientos
                    .Include(m => m.Generador) // Para obtener la información del Generador
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (movimientoExistente == null || movimientoExistente.ChoferId != int.Parse(_userManager.GetUserId(User)))
                {
                    return NotFound();
                }

                // Verificar si el estado es "Realizado" y si hay una foto cargada
                if (movimiento.EstadoId == 3 && foto != null)
                {
                    // Guardar la imagen en el servidor
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(foto.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await foto.CopyToAsync(fileStream);
                    }

                    movimientoExistente.Foto = $"/uploads/{uniqueFileName}";

                    // Notificación por correo al usuario solicitante
                    if (movimientoExistente.Generador != null && !string.IsNullOrEmpty(movimientoExistente.Generador.Email))
                    {
                        string emailBody = $"<p>Estimado," +
                            $"Tu solicitud de movimiento ha sido completada. Adjunto te dejamos la foto de la entrega.</p>";
                        await _emailService.SendEmailAsync(
                            movimientoExistente.Generador.Email,
                            "Solicitud de Movimiento Completada",
                            emailBody,
                            filePath
                        );
                    }
                }

                movimientoExistente.EstadoId = movimiento.EstadoId;

                _context.Update(movimientoExistente);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MisMovimientos));
            }

            ViewBag.Estados = _context.Estados.ToList();
            return View(movimiento);
        }
    }
}



