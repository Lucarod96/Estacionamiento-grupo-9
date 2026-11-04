using System.Security.Claims;
using Estacionamiento_grupo_9.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace Estacionamiento_grupo_9.Controllers;

[Authorize(Roles = Rol.Admin + "," + Rol.Operador)]
public class EstadiaController : Controller
{
    private readonly IRepositorioEstadia repositorioEstadia;
    private readonly IRepositorioPlaza repositorioPlaza;
    private readonly IRepositorioCliente repositorioCliente;

    private const int TAMANO_PAGINA = 10;

    public EstadiaController(
        IRepositorioEstadia repositorioEstadia,
        IRepositorioPlaza repositorioPlaza,
        IRepositorioCliente repositorioCliente)
    {
        this.repositorioEstadia = repositorioEstadia;
        this.repositorioPlaza = repositorioPlaza;
        this.repositorioCliente = repositorioCliente;
    }

    // Listado paginado: por defecto muestra solo los autos que están adentro
    [HttpGet]
    public IActionResult Index(
        string? busqueda, bool soloActivas = true, int pagina = 1)
    {
        string filtro = (busqueda ?? "").Trim();
        if (filtro.Length > 100) filtro = filtro[..100];
        if (pagina < 1) pagina = 1;

        int total = repositorioEstadia.ObtenerCantidad(filtro, soloActivas);
        int paginas = Math.Max(
            1, (int)Math.Ceiling((double)total / TAMANO_PAGINA));
        pagina = Math.Min(pagina, paginas);

        var items = repositorioEstadia.ObtenerLista(
            pagina, TAMANO_PAGINA, filtro, soloActivas);

        return View(new EstadiaIndexViewModel
        {
            Items = items,
            PaginaActual = pagina,
            TotalPaginas = paginas,
            TotalRegistros = total,
            Busqueda = filtro,
            SoloActivas = soloActivas
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new EstadiaIngresoViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(EstadiaIngresoViewModel modelo)
    {
        if (!ModelState.IsValid) return View(modelo);

        string patente = modelo.Patente.Trim().ToUpper();

        if (repositorioCliente.ObtenerPorPatente(patente) == null)
        {
            ModelState.AddModelError(nameof(modelo.Patente),
                "La patente no está registrada. Registrá primero al cliente.");
        }

        if (repositorioPlaza.ObtenerPorId(modelo.ID_Plaza) == null)
        {
            ModelState.AddModelError(nameof(modelo.ID_Plaza),
                "La plaza indicada no existe.");
        }
        else if (repositorioEstadia.PlazaOcupada(modelo.ID_Plaza))
        {
            ModelState.AddModelError(nameof(modelo.ID_Plaza),
                "Esa plaza ya está ocupada.");
        }

        if (repositorioEstadia.PatenteAdentro(patente))
        {
            ModelState.AddModelError(nameof(modelo.Patente),
                "Ese vehículo ya tiene una estadía activa.");
        }

        if (!ModelState.IsValid) return View(modelo);

        if (!TryObtenerIdStaff(out int idStaff)) return Challenge();

        try
        {
            repositorioEstadia.RegistrarIngreso(new Estadia
            {
                ID_Plaza = modelo.ID_Plaza,
                Patente = patente,
                ID_Staff = idStaff,
                Ingreso = DateTime.Now
            });
        }
        catch (MySqlException ex) when (ex.Number == 1452)
        {
            ModelState.AddModelError(string.Empty,
                "La plaza o el cliente ya no existen. Volvé a intentarlo.");
            return View(modelo);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            if (ex.Message.Contains("UX_estadia_patente_activa"))
            {
                ModelState.AddModelError(nameof(modelo.Patente),
                    "Ese vehículo acaba de ingresar. Revisá el listado.");
            }
            else
            {
                ModelState.AddModelError(nameof(modelo.ID_Plaza),
                    "Esa plaza acaba de ser ocupada. Elegí otra.");
            }

            return View(modelo);
        }

        TempData["SuccessMessage"] =
            $"Ingreso registrado: {patente} en la plaza {modelo.ID_Plaza}.";

        return RedirectToAction(nameof(Index));
    }

    // [HttpPost]
    // [ValidateAntiForgeryToken]
    // public IActionResult Salir(int id)
    // {
    //     var estadia = repositorioEstadia.ObtenerPorId(id);

    //     if (estadia == null) return NotFound();

    //     if (!estadia.Ocupado)
    //     {
    //         TempData["ErrorMessage"] = "Esa estadía ya estaba finalizada.";
    //         return RedirectToAction(nameof(Index));
    //     }

    //     var egreso = DateTime.Now;

    //     if (repositorioEstadia.RegistrarSalida(id, egreso) == 0)
    //     {
    //         TempData["ErrorMessage"] = "No se pudo registrar la salida.";
    //         return RedirectToAction(nameof(Index));
    //     }

    //     var permanencia = egreso - estadia.Ingreso;

    //     TempData["SuccessMessage"] =
    //         $"Salida registrada: {estadia.Patente}. " +
    //         $"Permanencia: {(int)permanencia.TotalHours} h {permanencia.Minutes} min.";

    //     return RedirectToAction(nameof(Index));
    // }

    // El ID del empleado sale del Claim (JWT o Cookie)
    private bool TryObtenerIdStaff(out int idStaff)
    {
        return int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier), out idStaff);
    }

    // Autocompletado de patente
    [HttpGet]
    public IActionResult BuscarCliente(string? termino)
    {
        string filtro = (termino ?? "").Trim();

        if (filtro.Length < 2 || filtro.Length > 50)
            return Ok(Array.Empty<object>());

        var clientes = repositorioCliente.BuscarLimitado(filtro, 10);

        return Ok(clientes.Select(c => new
        {
            patente = c.Patente,
            nombre = c.Nombre,
            apellido = c.Apellido
        }));
    }

    // Autocompletado de plaza
    [HttpGet]
    public IActionResult BuscarPlaza(string? termino)
    {
        string filtro = (termino ?? "").Trim();

        if (filtro.Length > 50)
            return Ok(Array.Empty<object>());

        var plazas = repositorioEstadia.BuscarPlazasLibres(filtro, 10);

        return Ok(plazas.Select(p => new
        {
            id = p.ID_Plaza,
            piso = p.Piso,
            sector = p.Sector,
            tipo = p.Tipo,
            precioXHora = p.PrecioXHora
        }));
    }

    // GET: api/estadia
    [HttpGet("api/estadia")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult ApiObtenerTodos([FromQuery] string? busqueda, [FromQuery] bool soloActivas = true, [FromQuery] int pagina = 1)
    {
        string filtro = (busqueda ?? "").Trim();
        var items = repositorioEstadia.ObtenerLista(pagina, TAMANO_PAGINA, filtro, soloActivas);
        return Ok(items);
    }

    // GET: api/estadia/5
    [HttpGet("api/estadia/{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult ApiObtenerPorId(int id)
    {
        var estadia = repositorioEstadia.ObtenerPorId(id);
        if (estadia == null) return NotFound(new { mensaje = "Estadía no encontrada." });

        return Ok(estadia);
    }

    // POST: api/estadia (Registrar Ingreso)
    [HttpPost("api/estadia")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult ApiRegistrarIngreso([FromBody] EstadiaIngresoViewModel modelo)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        string patente = modelo.Patente.Trim().ToUpper();

        if (repositorioCliente.ObtenerPorPatente(patente) == null)
            return BadRequest(new { mensaje = "La patente no está registrada." });

        if (repositorioPlaza.ObtenerPorId(modelo.ID_Plaza) == null)
            return BadRequest(new { mensaje = "La plaza indicada no existe." });

        if (repositorioEstadia.PlazaOcupada(modelo.ID_Plaza))
            return BadRequest(new { mensaje = "La plaza seleccionada ya está ocupada." });

        if (repositorioEstadia.PatenteAdentro(patente))
            return BadRequest(new { mensaje = "El vehículo ya posee una estadía activa." });

        if (!TryObtenerIdStaff(out int idStaff))
            return Unauthorized(new { mensaje = "No se pudo identificar al empleado emisor del token." });

        try
        {
            var estadia = new Estadia
            {
                ID_Plaza = modelo.ID_Plaza,
                Patente = patente,
                ID_Staff = idStaff,
                Ingreso = DateTime.Now
            };

            int idCreado = repositorioEstadia.RegistrarIngreso(estadia);
            return CreatedAtAction(nameof(ApiObtenerPorId), new { id = idCreado }, estadia);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al registrar el ingreso.", detalle = ex.Message });
        }
    }

    // POST: api/estadia/5/salida (Registrar Salida)
    // [HttpPost("api/estadia/{id}/salida")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    // public IActionResult ApiRegistrarSalida(int id)
    // {
    //     var estadia = repositorioEstadia.ObtenerPorId(id);
    //     if (estadia == null) return NotFound(new { mensaje = "Estadía no encontrada." });

    //     if (!estadia.Ocupado) return BadRequest(new { mensaje = "La estadía ya fue finalizada previamente." });

    //     var egreso = DateTime.Now;
    //     int filasAfectadas = repositorioEstadia.RegistrarSalida(id, egreso);

    //     if (filasAfectadas > 0)
    //     {
    //         var permanencia = egreso - estadia.Ingreso;
    //         return Ok(new
    //         {
    //             mensaje = "Salida registrada con éxito.",
    //             idEstadia = id,
    //             patente = estadia.Patente,
    //             egreso = egreso,
    //             horasPermanencia = (int)permanencia.TotalHours,
    //             minutosPermanencia = permanencia.Minutes
    //         });
    //     }

    //     return StatusCode(500, new { mensaje = "No se pudo registrar la salida en la base de datos." });
    // }

    // Las estadías se conservan en el historial.
    [HttpDelete("api/estadia/{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Rol.Admin)]
    public IActionResult ApiEliminar(int id)
    {
        Response.Headers["Allow"] = "GET";

        return StatusCode(405, new
        {
            mensaje = "No se permite eliminar estadías registradas. Se conserva el historial de ingresos y salidas."
        });
    }
}