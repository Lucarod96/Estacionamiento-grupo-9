using Estacionamiento_grupo_9.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Estacionamiento_grupo_9.Controllers;

[Authorize(Roles = Rol.Admin + "," + Rol.Operador)]
public class TipoDePlazaController : Controller
{
    private readonly IRepositorioTipoDePlaza repositorio;

    private const int TAMANO_PAGINA = 10;

    public TipoDePlazaController(
        IRepositorioTipoDePlaza repositorio)
    {
        this.repositorio = repositorio;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Listar(
        string? busqueda,
        int pagina = 1)
    {
        string filtro = (busqueda ?? "").Trim();

        if (filtro.Length > 100 ||
            pagina < 1 ||
            pagina > 100000)
        {
            return BadRequest(new
            {
                mensaje = "Los parámetros de búsqueda no son válidos."
            });
        }

        int total = repositorio.ObtenerCantidad(filtro);

        int paginas = Math.Max(
            1,
            (int)Math.Ceiling(
                (double)total / TAMANO_PAGINA));

        pagina = Math.Min(pagina, paginas);

        var items = repositorio.ObtenerLista(
            pagina,
            TAMANO_PAGINA,
            filtro);

        return Ok(new
        {
            items,
            total,
            pagina,
            paginas
        });
    }

    [HttpGet]
    public IActionResult Buscar(
        string? q,
        int pagina = 1)
    {
        string filtro = (q ?? "").Trim();

        if (filtro.Length > 100 ||
            pagina < 1 ||
            pagina > 100000)
        {
            return BadRequest(new
            {
                mensaje = "Los parámetros de búsqueda no son válidos."
            });
        }

        if (filtro.Length < 2)
        {
            return Ok(new
            {
                items = Array.Empty<TipoDePlaza>(),
                hayMas = false
            });
        }

        var items = repositorio.ObtenerLista(
            pagina,
            TAMANO_PAGINA,
            filtro);

        int total = repositorio.ObtenerCantidad(filtro);

        bool hayMas =
            (long)pagina * TAMANO_PAGINA < total;

        return Ok(new
        {
            items,
            hayMas
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Rol.Admin)]
    public IActionResult Guardar(
        [FromBody] TipoDePlaza? modelo,
        bool nuevo = false)
    {
        if (modelo == null)
        {
            return BadRequest(new
            {
                mensaje = "No se recibieron datos válidos."
            });
        }

        if (!ModelState.IsValid)
        {
            var errores = ModelState.Values
                .SelectMany(valor => valor.Errors)
                .Select(error => error.ErrorMessage);

            return BadRequest(new
            {
                mensaje = string.Join(" ", errores)
            });
        }

        if (decimal.Round(modelo.PrecioXHora, 2)
            != modelo.PrecioXHora)
        {
            return BadRequest(new
            {
                mensaje = "El precio admite como máximo dos decimales."
            });
        }

        modelo.Tipo = modelo.Tipo.Trim();

        try
        {
            if (nuevo)
            {
                var existente = repositorio.ObtenerPorId(
                    modelo.ID_TipoDePlaza);

                if (existente != null)
                {
                    return Conflict(new
                    {
                        mensaje = "El ID ya existe. Elegí otro número."
                    });
                }

                repositorio.Alta(modelo);
            }
            else
            {
                var existente = repositorio.ObtenerPorId(
                    modelo.ID_TipoDePlaza);

                if (existente == null)
                {
                    return NotFound(new
                    {
                        mensaje = "El tipo de plaza ya no existe."
                    });
                }

                repositorio.Modificacion(modelo);
            }

            return Ok(new
            {
                mensaje = "Tipo de plaza guardado correctamente."
            });
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new
            {
                mensaje = "El ID ya está registrado."
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Rol.Admin)]
    public IActionResult Eliminar(int id)
    {
        if (id < 1)
        {
            return BadRequest(new
            {
                mensaje = "El ID debe ser mayor que cero."
            });
        }

        try
        {
            int resultado = repositorio.Baja(id);

            if (resultado == 0)
            {
                return NotFound(new
                {
                    mensaje = "El tipo de plaza ya no existe."
                });
            }

            return Ok(new
            {
                mensaje = "Tipo de plaza eliminado correctamente."
            });
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            return Conflict(new
            {
                mensaje =
                    "No se puede eliminar el tipo porque tiene plazas asociadas."
            });
        }
    }
}