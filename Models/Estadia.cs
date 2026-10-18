using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Estacionamiento_grupo_9.Models;

public class Estadia
{
    [JsonPropertyName("id_Estadia")]
    public int ID_Estadia { get; set; }

    [JsonPropertyName("id_Plaza")]
    [Range(1, int.MaxValue, ErrorMessage = "Debés seleccionar una plaza.")]
    public int ID_Plaza { get; set; }

    [Required(ErrorMessage = "La patente es obligatoria.")]
    [StringLength(10, MinimumLength = 6,
        ErrorMessage = "La patente debe tener entre 6 y 10 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9\s-]+$",
        ErrorMessage = "La patente solo puede tener letras, números y guiones.")]
    public string Patente { get; set; } = "";

    [JsonPropertyName("id_Staff")]
    public int ID_Staff { get; set; }

    public DateTime Ingreso { get; set; }

    public DateTime? Egreso { get; set; }

    public bool Ocupado { get; set; }

    // Solo lectura: vienen de los JOIN, no se guardan en esta tabla
    public int Piso { get; set; }
    public string Sector { get; set; } = "";
    public string Tipo { get; set; } = "";
    public decimal PrecioXHora { get; set; }
}