using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Estacionamiento_grupo_9.Models
{
    // Usuario del sistema (empleados). Hereda de Identity con clave int:
    // Id, UserName, Email, PasswordHash, etc. ya vienen de IdentityUser<int>.
    public class Staff : IdentityUser<int>
    {
        [StringLength(250)]
        public string? AvatarUrl { get; set; }
    }
}
