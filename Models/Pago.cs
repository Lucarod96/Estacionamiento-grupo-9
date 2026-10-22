using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Estacionamiento_grupo_9.Models
{
    public class Pago
    {
        [Key]
        [Display(Name = "N° Pago")]
        public int ID_Pago { get; set; }

        [Required(ErrorMessage = "La estadía es obligatoria.")]
        [Display(Name = "N° Estadía")]
        public int ID_Estadia { get; set; }

        [Required]
        [Display(Name = "Fecha de Emisión")]
        [DataType(DataType.DateTime)]
        public DateTime FechaEmision { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El precio por hora es obligatorio.")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Tarifa Aplicada ($/hs)")]
        public decimal PrecioXHoraAplicado { get; set; }

        [Required(ErrorMessage = "El monto total es obligatorio.")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Monto Total ($)")]
        public decimal MontoTotal { get; set; }

        [StringLength(255)]
        [Display(Name = "Comprobante Adjunto")]
        public string? ComprobanteUrl { get; set; }

        // Propiedad de navegación opcional
        public Estadia? Estadia { get; set; }
    }
}