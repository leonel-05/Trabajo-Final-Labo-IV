using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace SolicitudMovimientoAppWeb_MVC.Models
{
    public class ApplicationDbContext : IdentityDbContext<Usuario, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
        {

        }

        public DbSet<Movimiento> Movimientos { get; set; }
        public DbSet<TipoDeEmbalaje> TiposDeEmbalaje { get; set; }
        public DbSet<Estado> Estados { get; set; }
        public DbSet<Lugares> Lugares { get; set; }

    }
}
