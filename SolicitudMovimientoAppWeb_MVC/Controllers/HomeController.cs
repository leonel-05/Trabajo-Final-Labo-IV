using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SolicitudMovimientoAppWeb_MVC.Models;
using X.PagedList.Extensions;

namespace SolicitudMovimientoAppWeb_MVC.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Home/Index
        public async Task<IActionResult> Index(int? page, int pageSize = 10)
        {
            var movimientosQuery = _context.Movimientos
                .Include(m => m.LugarSalida)
                .Include(m => m.LugarDestino)
                .Include(m => m.Estado)
                .OrderByDescending(m => m.FechaSolicitud);

            // Obtiene el número total de elementos
            int totalItems = await movimientosQuery.CountAsync();

            // Número de página actual
            int pageNumber = page ?? 1;

            // Obtener solo la página solicitada de elementos
            var movimientos = await movimientosQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pasar valores de paginación a la vista
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.CurrentPage = pageNumber;

            return View(movimientos);
        }

    }
}


