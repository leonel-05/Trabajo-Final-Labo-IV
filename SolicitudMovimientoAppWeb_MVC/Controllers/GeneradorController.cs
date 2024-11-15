using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolicitudMovimientoAppWeb_MVC.Models;
using X.PagedList.Extensions;

namespace SolicitudMovimientoAppWeb_MVC.Controllers
{
    [Authorize(Roles = "Generador")]
    public class GeneradorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        public GeneradorController(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //GET: Generador/Create
        public IActionResult Create()
        {
            LoadSelectLists();
            return View();
        }

        //POST: Generador/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movimiento movimiento)
        {
            if (ModelState.IsValid)
            {
                movimiento.EstadoId = 1;
                movimiento.FechaSolicitud = DateTime.Now;
                movimiento.GeneradorId = int.Parse(_userManager.GetUserId(User));

                _context.Movimientos.Add(movimiento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadSelectLists();
            return View(movimiento);
        }

        //GET: Generador/Index
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            var userId = int.Parse(_userManager.GetUserId(User));
            var movimientos = await _context.Movimientos
                .Include(m => m.LugarSalida)
                .Include(m => m.LugarDestino)
                .Include(m => m.Estado)
                .Where(m => m.GeneradorId == userId)
                .OrderByDescending(m => m.FechaSolicitud)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalItems = await _context.Movimientos.CountAsync(m => m.GeneradorId == userId);
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = page;

            return View(movimientos);
        }


        // GET: Generador/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento == null)
            {
                return NotFound();
            }

            LoadSelectLists();
            return View(movimiento);
        }

        // POST: Generador/Edit/5
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
                    if (movimientoExistente == null || movimientoExistente.GeneradorId != int.Parse(_userManager.GetUserId(User)))
                    {
                        return NotFound();
                    }

                    movimientoExistente.LugarSalidaId = movimiento.LugarSalidaId;
                    movimientoExistente.LugarDestinoId = movimiento.LugarDestinoId;
                    movimientoExistente.TipoDeEmbalajeId = movimiento.TipoDeEmbalajeId;
                    movimientoExistente.Descripcion = movimiento.Descripcion;

                    _context.Update(movimientoExistente);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Error al actualizar el movimiento: {ex.Message}");
                }
            }

            LoadSelectLists();
            return View(movimiento);
        }

        private void LoadSelectLists()
        {
            ViewBag.Lugares = _context.Lugares?.ToList() ?? new List<Lugares>();
            ViewBag.TipoDeEmbalaje = _context.TiposDeEmbalaje?.ToList() ?? new List<TipoDeEmbalaje>();
        }

        // GET: Generador/Upload
        public IActionResult Upload()
        {
            return View();
        }

        // POST: Generador/Upload
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var userId = int.Parse(_userManager.GetUserId(User)); // Obtener el ID del usuario actualmente autenticado

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RangeUsed().RowsUsed();

                        var movimientos = new List<Movimiento>();

                        foreach (var row in rows.Skip(1)) // Skip header row
                        {
                            var movimiento = new Movimiento
                            {
                                LugarSalidaId = int.TryParse(row.Cell(1).GetString(), out int lugarSalidaId) ? lugarSalidaId : 0,
                                LugarDestinoId = int.TryParse(row.Cell(2).GetString(), out int lugarDestinoId) ? lugarDestinoId : 0,
                                FechaEntrega = DateTime.TryParse(row.Cell(3).GetString(), out DateTime fechaEntrega) ? fechaEntrega : DateTime.MinValue,
                                EstadoId = int.TryParse(row.Cell(4).GetString(), out int estadoId) ? estadoId : 0,
                                Descripcion = row.Cell(5).GetString(),
                                Cantidad = int.TryParse(row.Cell(6).GetString(), out int cantidad) ? cantidad : 0,
                                CodigoMaterial = row.Cell(7).GetString(),
                                TipoDeEmbalajeId = int.TryParse(row.Cell(8).GetString(), out int tipoDeEmbalajeId) ? tipoDeEmbalajeId : 0,
                                GeneradorId = userId // Asignar el ID del usuario actualmente autenticado
                            };
                            movimientos.Add(movimiento);
                        }

                        await _context.Movimientos.AddRangeAsync(movimientos);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            return RedirectToAction("Index");
        }
    }
}
