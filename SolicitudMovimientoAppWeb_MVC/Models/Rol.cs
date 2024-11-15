using System.ComponentModel.DataAnnotations;

namespace SolicitudMovimientoAppWeb_MVC.Models
{
    public class Rol
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nombre { get; set; }

        public ICollection<Usuario> Usuarios { get; set; }
    }
}
