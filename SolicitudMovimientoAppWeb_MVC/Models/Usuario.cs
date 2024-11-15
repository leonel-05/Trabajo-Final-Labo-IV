using Microsoft.AspNetCore.Identity;

namespace SolicitudMovimientoAppWeb_MVC.Models
{
    public class Usuario : IdentityUser<int>
    {
        public string Nombre { get; set; }

    }
}
