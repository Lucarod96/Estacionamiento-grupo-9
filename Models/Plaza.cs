using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Estacionamiento_grupo_9.Models;

public class Plaza
{
    [JsonPropertyName("id_Plaza")]
    [Range(1, int.MaxValue,
        ErrorMessage = "El ID debe ser un número mayor que cero.")]
    public int ID_Plaza { get; set; }

    [JsonPropertyName("id_TipoDePlaza")]
    [Range(1, int.MaxValue,
        ErrorMessage = "Debés seleccionar un tipo de plaza.")]
    public int ID_TipoDePlaza { get; set; }

    [Range(0, 200,
        ErrorMessage = "El piso debe estar entre 0 y 200.")]
    public int Piso { get; set; }

    [Required(ErrorMessage = "El sector es obligatorio.")]
    [StringLength(45,
        ErrorMessage = "El sector no puede superar los 45 caracteres.")]
    public string Sector { get; set; } = "";

    public string Tipo { get; set; } = "";

    public decimal PrecioXHora { get; set; }
}