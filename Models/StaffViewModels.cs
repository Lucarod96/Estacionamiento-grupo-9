using System.ComponentModel.DataAnnotations;

namespace Estacionamiento_grupo_9.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }
    }

    public class StaffCreateViewModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres.")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "Solo letras, números, punto, guion y guion bajo.")]
        [Display(Name = "Usuario")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato de email no es válido.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Repetí la contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Repetir contraseña")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccioná un rol.")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = Estacionamiento_grupo_9.Models.Rol.Operador;

        [Display(Name = "Avatar")]
        public IFormFile? AvatarFile { get; set; }
    }

    public class StaffEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Usuario")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato de email no es válido.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Seleccioná un rol.")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        [Display(Name = "Nuevo avatar")]
        public IFormFile? AvatarFile { get; set; }
    }

    public class PerfilViewModel
    {
        [Display(Name = "Usuario")]
        public string? UserName { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Rol")]
        public string? Rol { get; set; }

        public string? AvatarUrl { get; set; }

        [Display(Name = "Nuevo avatar")]
        public IFormFile? AvatarFile { get; set; }
    }

    public class StaffListItem
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

    public class StaffIndexViewModel
    {
        public IList<StaffListItem> Items { get; set; } = new List<StaffListItem>();
        public int PaginaActual { get; set; } = 1;
        public int TotalPaginas { get; set; } = 1;
        public int TotalRegistros { get; set; }
    }
}
