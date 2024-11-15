using System.ComponentModel.DataAnnotations;

namespace SolicitudMovimientoAppWeb_MVC.Models
{
    public class TipoDeEmbalaje
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Descripcion { get; set; }
    }
}
