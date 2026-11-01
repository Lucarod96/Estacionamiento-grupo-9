using Estacionamiento_grupo_9.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Linq;

namespace Estacionamiento_grupo_9.Controllers;

[Authorize(Roles = Rol.Admin + "," + Rol.Operador)]
public class PlazaController : Controller
{
    private readonly IRepositorioPlaza repositorioPlaza;
    private readonly IRepositorioTipoDePlaza repositorioTipo;

    private const int TAMANO_PAGINA = 10;

    public PlazaController(
        IRepositorioPlaza repositorioPlaza,
        IRepositorioTipoDePlaza repositorioTipo)
    {
        this.repositorioPlaza = repositorioPlaza;
        this.repositorioTipo = repositorioTipo;
    }

    // ==========================================
    // === VISTAS Y AJAX MVC (Autenticación Cookie) ===
    // ==========================================

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

        int total = repositorioPlaza.ObtenerCantidad(filtro);

        int paginas = Math.Max(
            1,
            (int)Math.Ceiling((double)total / TAMANO_PAGINA));

        pagina = Math.Min(pagina, paginas);

        var items = repositorioPlaza.ObtenerLista(
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Rol.Admin)]
    public IActionResult Guardar(
        [FromBody] Plaza? modelo,
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

        modelo.Sector = modelo.Sector.Trim();

        try
        {
            var tipo = repositorioTipo.ObtenerPorId(
                modelo.ID_TipoDePlaza);

            if (tipo == null)
            {
                return BadRequest(new
                {
                    mensaje = "El tipo de plaza seleccionado no existe."
                });
            }

            if (nuevo)
            {
                var existente = repositorioPlaza.ObtenerPorId(
                    modelo.ID_Plaza);

                if (existente != null)
                {
                    return Conflict(new
                    {
                        mensaje = "El ID de plaza ya existe. Elegí otro número."
                    });
                }

                repositorioPlaza.Alta(modelo);
            }
            else
            {
                var existente = repositorioPlaza.ObtenerPorId(
                    modelo.ID_Plaza);

                if (existente == null)
                {
                    return NotFound(new
                    {
                        mensaje = "La plaza ya no existe."
                    });
                }

                repositorioPlaza.Modificacion(modelo);
            }

            return Ok(new
            {
                mensaje = "Plaza guardada correctamente."
            });
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new
            {
                mensaje = "El ID de plaza ya está registrado."
            });
        }
        catch (MySqlException ex) when (ex.Number == 1452)
        {
            return Conflict(new
            {
                mensaje = "El tipo seleccionado ya no existe. Volvé a buscarlo."
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
            int resultado = repositorioPlaza.Baja(id);

            if (resultado == 0)
            {
                return NotFound(new
                {
                    mensaje = "La plaza ya no existe."
                });
            }

            return Ok(new
            {
                mensaje = "Plaza eliminada correctamente."
            });
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            return Conflict(new
            {
                mensaje = "No se puede eliminar la plaza porque tiene registros asociados."
            });
        }
    }

    // =========================================================================
    // === ENDPOINTS WEB API REST (Autenticación JWT para Postman / Clientes) ===
    // =========================================================================

    // GET: api/plaza
    [HttpGet("api/plaza")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult ApiObtenerTodos([FromQuery] string? busqueda, [FromQuery] int pagina = 1)
    {
        string filtro = (busqueda ?? "").Trim();
        if (filtro.Length > 100 || pagina < 1) return BadRequest(new { mensaje = "Los parámetros de búsqueda no son válidos." });

        int total = repositorioPlaza.ObtenerCantidad(filtro);
        int paginas = Math.Max(1, (int)Math.Ceiling((double)total / TAMANO_PAGINA));
        pagina = Math.Min(pagina, paginas);

        var items = repositorioPlaza.ObtenerLista(pagina, TAMANO_PAGINA, filtro);
        return Ok(new { items, total, pagina, paginas });
    }

    // GET: api/plaza/5
    [HttpGet("api/plaza/{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult ApiObtenerPorId(int id)
    {
        var plaza = repositorioPlaza.ObtenerPorId(id);
        if (plaza == null) return NotFound(new { mensaje = "Plaza no encontrada." });

        return Ok(plaza);
    }

    // POST: api/plaza (Restringido solo a Administradores)
    [HttpPost("api/plaza")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Rol.Admin)]
    public IActionResult ApiCrear([FromBody] Plaza? modelo)
    {
        if (modelo == null) return BadRequest(new { mensaje = "No se recibieron datos válidos." });

        if (!ModelState.IsValid)
        {
            var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { mensaje = string.Join(" ", errores) });
        }

        modelo.Sector = modelo.Sector.Trim();

        var tipo = repositorioTipo.ObtenerPorId(modelo.ID_TipoDePlaza);
        if (tipo == null) return BadRequest(new { mensaje = "El tipo de plaza seleccionado no existe." });

        var existente = repositorioPlaza.ObtenerPorId(modelo.ID_Plaza);
        if (existente != null) return Conflict(new { mensaje = "El ID de plaza ya existe." });

        try
        {
            repositorioPlaza.Alta(modelo);
            return CreatedAtAction(nameof(ApiObtenerPorId), new { id = modelo.ID_Plaza }, modelo);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new { mensaje = "El ID de plaza ya está registrado." });
        }
    }

    // PUT: api/plaza/5 (Restringido solo a Administradores)
    [HttpPut("api/plaza/{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Rol.Admin)]
    public IActionResult ApiModificar(int id, [FromBody] Plaza? modelo)
    {
        if (modelo == null || id != modelo.ID_Plaza) return BadRequest(new { mensaje = "El ID especificado no coincide o los datos son nulos." });

        if (!ModelState.IsValid)
        {
            var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { mensaje = string.Join(" ", errores) });
        }

        modelo.Sector = modelo.Sector.Trim();

        var existente = repositorioPlaza.ObtenerPorId(id);
        if (existente == null) return NotFound(new { mensaje = "La plaza no existe." });

        var tipo = repositorioTipo.ObtenerPorId(modelo.ID_TipoDePlaza);
        if (tipo == null) return BadRequest(new { mensaje = "El tipo de plaza seleccionado no existe." });

        try
        {
            repositorioPlaza.Modificacion(modelo);
            return Ok(new { mensaje = "Plaza guardada correctamente.", plaza = modelo });
        }
        catch (MySqlException ex) when (ex.Number == 1452)
        {
            return Conflict(new { mensaje = "El tipo seleccionado ya no existe." });
        }
    }

    // DELETE: api/plaza/5 (Restringido solo a Administradores)
    [HttpDelete("api/plaza/{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Rol.Admin)]
    public IActionResult ApiEliminar(int id)
    {
        if (id < 1) return BadRequest(new { mensaje = "El ID debe ser mayor que cero." });

        try
        {
            int resultado = repositorioPlaza.Baja(id);
            if (resultado == 0) return NotFound(new { mensaje = "La plaza ya no existe." });

            return Ok(new { mensaje = "Plaza eliminada correctamente." });
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            return Conflict(new { mensaje = "No se puede eliminar la plaza porque tiene registros asociados." });
        }
    }
}