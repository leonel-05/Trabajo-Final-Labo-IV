using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SolicitudMovimientoAppWeb_MVC.Models
{
    public class Movimiento
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Generador")]
        public int GeneradorId { get; set; }
        public Usuario? Generador { get; set; }

        [ForeignKey("Chofer")]
        public int? ChoferId { get; set; }
        public Usuario? Chofer { get; set; }

        [Required]
        public DateTime FechaSolicitud { get; set; }

        public DateTime? FechaEntrega { get; set; }

        [Required]
        public int LugarSalidaId { get; set; }
        public Lugares? LugarSalida { get; set; }

        [Required]
        public int LugarDestinoId { get; set; }
        public Lugares? LugarDestino { get; set; }

        [Required]
        public string? Descripcion { get; set; }

        [Required]
        public string? CodigoMaterial { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [ForeignKey("TipoDeEmbalaje")]
        public int TipoDeEmbalajeId { get; set; }
        public TipoDeEmbalaje? TipoDeEmbalaje { get; set; }

        [ForeignKey("Estado")]
        public int EstadoId { get; set; }
        public Estado? Estado { get; set; }

        public string? Foto { get; set; }
    }
}
