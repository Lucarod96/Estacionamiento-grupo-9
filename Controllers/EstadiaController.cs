using System.Security.Claims;
using Estacionamiento_grupo_9.Models;
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
            // Otro empleado ganó de mano entre la validación y el guardado
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Salir(int id)
    {
        var estadia = repositorioEstadia.ObtenerPorId(id);

        if (estadia == null) return NotFound();

        if (!estadia.Ocupado)
        {
            TempData["ErrorMessage"] = "Esa estadía ya estaba finalizada.";
            return RedirectToAction(nameof(Index));
        }

        var egreso = DateTime.Now;

        if (repositorioEstadia.RegistrarSalida(id, egreso) == 0)
        {
            TempData["ErrorMessage"] = "No se pudo registrar la salida.";
            return RedirectToAction(nameof(Index));
        }

        var permanencia = egreso - estadia.Ingreso;

        TempData["SuccessMessage"] =
            $"Salida registrada: {estadia.Patente}. " +
            $"Permanencia: {(int)permanencia.TotalHours} h {permanencia.Minutes} min.";

        return RedirectToAction(nameof(Index));
    }

    // El ID del empleado sale de la sesión, nunca del formulario
    private bool TryObtenerIdStaff(out int idStaff)
    {
        return int.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier), out idStaff);
    }

    // Autocompletado de patente: mínimo 2 caracteres, máximo 10 resultados
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

    // Autocompletado de plaza: solo plazas libres, máximo 10 resultados
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
}