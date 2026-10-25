using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Estacionamiento_grupo_9.Models;

public class TipoDePlaza
{
    [JsonPropertyName("id_TipoDePlaza")]
    [Range(1, int.MaxValue,
        ErrorMessage = "El ID debe ser un número mayor que cero.")]
    public int ID_TipoDePlaza { get; set; }

    [Required(ErrorMessage = "El tipo de plaza es obligatorio.")]
    [StringLength(45,
        ErrorMessage = "El tipo no puede superar los 45 caracteres.")]
    public string Tipo { get; set; } = "";

    [Range(
        typeof(decimal),
        "0.01",
        "99999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "El precio debe estar entre 0,01 y 99999999,99."
    )]
    public decimal PrecioXHora { get; set; }
}