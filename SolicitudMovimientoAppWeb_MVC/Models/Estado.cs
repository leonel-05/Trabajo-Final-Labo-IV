using System.ComponentModel.DataAnnotations;

namespace SolicitudMovimientoAppWeb_MVC.Models
{
    public class Estado
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Descripcion { get; set; }
    }
}
