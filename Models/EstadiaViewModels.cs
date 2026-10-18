using System.ComponentModel.DataAnnotations;

namespace Estacionamiento_grupo_9.Models;

// Solo lo que el empleado completa al dar entrada
public class EstadiaIngresoViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Debés indicar una plaza.")]
    [Display(Name = "Plaza")]
    public int ID_Plaza { get; set; }

    [Required(ErrorMessage = "La patente es obligatoria.")]
    [StringLength(10, MinimumLength = 6,
        ErrorMessage = "La patente debe tener entre 6 y 10 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9\s-]+$",
        ErrorMessage = "La patente solo puede tener letras, números y guiones.")]
    [Display(Name = "Patente")]
    public string Patente { get; set; } = "";
}

public class EstadiaIndexViewModel
{
    public IList<Estadia> Items { get; set; } = new List<Estadia>();
    public int PaginaActual { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }
    public string Busqueda { get; set; } = "";
    public bool SoloActivas { get; set; } = true;
}