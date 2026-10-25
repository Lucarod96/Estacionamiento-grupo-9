using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Estacionamiento_grupo_9.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Estacionamiento_grupo_9.Controllers
{
    [Authorize]
    public class PagoController : Controller
    {
        private readonly IRepositorioPago repositorioPago;
        private readonly IRepositorioEstadia repositorioEstadia;
        private readonly IRepositorioPlaza repositorioPlaza;
        private readonly IRepositorioTipoDePlaza repositorioTipoDePlaza;
        private readonly IWebHostEnvironment environment;
        private const int TAMANO_PAGINA = 10;

        public PagoController(
            IRepositorioPago repositorioPago,
            IRepositorioEstadia repositorioEstadia,
            IRepositorioPlaza repositorioPlaza,
            IRepositorioTipoDePlaza repositorioTipoDePlaza,
            IWebHostEnvironment environment)
        {
            this.repositorioPago = repositorioPago;
            this.repositorioEstadia = repositorioEstadia;
            this.repositorioPlaza = repositorioPlaza;
            this.repositorioTipoDePlaza = repositorioTipoDePlaza;
            this.environment = environment;
        }

        // GET: Pago
public IActionResult Index(string? busqueda, int pagina = 1)
{
    string filtro = (busqueda ?? "").Trim();

    if (filtro.Length > 100)
    {
        return BadRequest(
            "La búsqueda admite hasta 100 caracteres.");
    }

    int totalRegistros =
        repositorioPago.ObtenerCantidad(filtro);

    int totalPaginas = Math.Max(
        1,
        (int)Math.Ceiling(
            (double)totalRegistros / TAMANO_PAGINA));

    pagina = Math.Clamp(pagina, 1, totalPaginas);

    var pagos = repositorioPago.ObtenerLista(
        pagina,
        TAMANO_PAGINA,
        filtro);

    foreach (var pago in pagos)
    {
        pago.Estadia =
            repositorioEstadia.ObtenerPorId(pago.ID_Estadia);
    }

    ViewBag.PaginaActual = pagina;
    ViewBag.TotalPaginas = totalPaginas;
    ViewBag.TotalRegistros = totalRegistros;
    ViewBag.Busqueda = filtro;

    return View(pagos);
}

        // GET: Pago/Cobrar/5 (Cobra una estadía activa)
        public IActionResult Cobrar(int id)
        {
            var estadia = repositorioEstadia.ObtenerPorId(id);
            if (estadia == null) return NotFound();

            // Verificar si ya fue pagada
            var pagoExistente = repositorioPago.ObtenerPorEstadiaId(id);
            if (pagoExistente != null)
            {
                TempData["ErrorMessage"] = "Esta estadía ya fue cobrada previamente.";
                return RedirectToAction(nameof(Index));
            }

            // Cargar Plaza y TipoDePlaza para calcular la tarifa
            var plaza = repositorioPlaza.ObtenerPorId(estadia.ID_Plaza);
            if (plaza == null) return BadRequest("La plaza asociada no existe.");

            var tipoPlaza = repositorioTipoDePlaza.ObtenerPorId(plaza.ID_TipoDePlaza);
            if (tipoPlaza == null) return BadRequest("El tipo de plaza no existe.");

            DateTime fechaEgreso = DateTime.Now;
            TimeSpan duracion = fechaEgreso - estadia.Ingreso;

            // Cálculo: Mínimo 1 hora, fracción cuenta como hora entera
            int horasACobrar = (int)Math.Ceiling(duracion.TotalHours);
            if (horasACobrar < 1) horasACobrar = 1;

            decimal montoCalculado = horasACobrar * tipoPlaza.PrecioXHora;

            var pago = new Pago
            {
                ID_Estadia = estadia.ID_Estadia,
                FechaEmision = fechaEgreso,
                PrecioXHoraAplicado = tipoPlaza.PrecioXHora,
                MontoTotal = montoCalculado,
                Estadia = estadia
            };

            ViewBag.HorasCobradas = horasACobrar;
            ViewBag.DuracionReal = $"{(int)duracion.TotalHours}h {duracion.Minutes}m";

            return View(pago);
        }

        // POST: Pago/Cobrar
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Cobrar(
    int ID_Estadia,
    IFormFile? comprobanteFile)
{
    if (ID_Estadia <= 0)
        return BadRequest("La estadía no es válida.");

    var estadia = repositorioEstadia.ObtenerPorId(ID_Estadia);

    if (estadia == null)
        return NotFound();

    // Volver a comprobar antes de guardar.
    var pagoExistente =
        repositorioPago.ObtenerPorEstadiaId(ID_Estadia);

    if (pagoExistente != null)
    {
        TempData["ErrorMessage"] =
            "Esta estadía ya fue cobrada previamente.";

        return RedirectToAction(nameof(Index));
    }

    if (estadia.Egreso != null)
    {
        TempData["ErrorMessage"] =
            "No se puede cobrar una estadía que ya tiene salida.";

        return RedirectToAction("Index", "Estadia");
    }

    var plaza =
        repositorioPlaza.ObtenerPorId(estadia.ID_Plaza);

    if (plaza == null)
        return BadRequest("La plaza asociada no existe.");

    var tipoPlaza =
        repositorioTipoDePlaza.ObtenerPorId(
            plaza.ID_TipoDePlaza);

    if (tipoPlaza == null)
        return BadRequest("El tipo de plaza no existe.");

    if (tipoPlaza.PrecioXHora <= 0)
        return BadRequest("La tarifa debe ser mayor que cero.");

    // Todos los valores del cobro se calculan en el servidor.
    DateTime fechaEgreso = DateTime.Now;

    if (estadia.Ingreso > fechaEgreso)
        return BadRequest("La fecha de ingreso está en el futuro.");

    TimeSpan duracion = fechaEgreso - estadia.Ingreso;

    double horas = Math.Max(
        1,
        Math.Ceiling(duracion.TotalHours));

    decimal montoCalculado =
        (decimal)horas * tipoPlaza.PrecioXHora;

    if (montoCalculado > 99999999.99m)
        return BadRequest("El importe supera el máximo permitido.");

    var pago = new Pago
    {
        ID_Estadia = estadia.ID_Estadia,
        FechaEmision = fechaEgreso,
        PrecioXHoraAplicado = tipoPlaza.PrecioXHora,
        MontoTotal = montoCalculado,
        ComprobanteUrl = null,
        Estadia = estadia
    };

    // Se mantiene la subida actual.
    // La validación del archivo será el siguiente cambio.
    if (comprobanteFile != null && comprobanteFile.Length > 0)
    {
        string uploadsFolder = Path.Combine(
            environment.WebRootPath,
            "uploads",
            "comprobantes");

        Directory.CreateDirectory(uploadsFolder);

        string uniqueFileName =
            $"{Guid.NewGuid()}_{Path.GetFileName(comprobanteFile.FileName)}";

        string filePath = Path.Combine(
            uploadsFolder,
            uniqueFileName);

        using (var fileStream =
            new FileStream(filePath, FileMode.CreateNew))
        {
            await comprobanteFile.CopyToAsync(fileStream);
        }

        pago.ComprobanteUrl =
            $"/uploads/comprobantes/{uniqueFileName}";
    }

    int idPago = repositorioPago.Alta(pago);

    if (idPago > 0)
    {
        repositorioEstadia.RegistrarSalida(
            pago.ID_Estadia,
            pago.FechaEmision);

        TempData["SuccessMessage"] =
            $"Cobro N° {idPago} registrado exitosamente. " +
            "La estadía fue finalizada.";

        return RedirectToAction(nameof(Index));
    }

    ViewBag.HorasCobradas = horas;
    ViewBag.DuracionReal =
        $"{(int)duracion.TotalHours}h {duracion.Minutes}m";

    ModelState.AddModelError(
        "",
        "Ocurrió un error al registrar el pago.");

    return View(pago);
}

        // GET: Pago/Details/5
        public IActionResult Details(int id)
        {
            var pago = repositorioPago.ObtenerPorId(id);
            if (pago == null) return NotFound();

            pago.Estadia = repositorioEstadia.ObtenerPorId(pago.ID_Estadia);
            return View(pago);
        }
    }
}