using System.ComponentModel.DataAnnotations;

namespace Estacionamiento_grupo_9.Models
{
    public class Cliente
    {
        [Key]
        [Required(ErrorMessage = "La patente es obligatoria.")]
        [StringLength(10, MinimumLength = 6, ErrorMessage = "La patente debe tener entre 6 y 10 caracteres.")]
        [Display(Name = "Patente del Vehículo")]
        [RegularExpression(@"^[A-Z0-9\s-]+$", ErrorMessage = "La patente solo puede contener letras mayúsculas, números y guiones.")]
        public string Patente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(50, ErrorMessage = "El apellido no puede superar los 50 caracteres.")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [Phone(ErrorMessage = "El formato de teléfono no es válido.")]
        [StringLength(20, ErrorMessage = "El teléfono no puede superar los 20 caracteres.")]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        // Constructor vacío
        public Cliente() { }

        // Constructor parametrizado opcional
        public Cliente(string patente, string nombre, string apellido, string telefono)
        {
            Patente = patente;
            Nombre = nombre;
            Apellido = apellido;
            Telefono = telefono;
        }
    }
}