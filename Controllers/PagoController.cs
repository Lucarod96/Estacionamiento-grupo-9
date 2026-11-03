using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Estacionamiento_grupo_9.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Estacionamiento_grupo_9.Controllers
{
    [Authorize(Roles = Rol.Admin + "," + Rol.Operador)]
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

        // ==========================================
        // === VISTAS MVC (Autenticación Web Cookie) ===
        // ==========================================

        // GET: Pago
        public IActionResult Index(string? busqueda, int pagina = 1)
        {
            string filtro = (busqueda ?? "").Trim();

            if (filtro.Length > 100)
            {
                return BadRequest("La búsqueda admite hasta 100 caracteres.");
            }

            int totalRegistros = repositorioPago.ObtenerCantidad(filtro);

            int totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling((double)totalRegistros / TAMANO_PAGINA));

            pagina = Math.Clamp(pagina, 1, totalPaginas);

            var pagos = repositorioPago.ObtenerLista(
                pagina,
                TAMANO_PAGINA,
                filtro);

            foreach (var pago in pagos)
            {
                pago.Estadia = repositorioEstadia.ObtenerPorId(pago.ID_Estadia);
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

            var pagoExistente = repositorioPago.ObtenerPorEstadiaId(id);
            if (pagoExistente != null)
            {
                TempData["ErrorMessage"] = "Esta estadía ya fue cobrada previamente.";
                return RedirectToAction(nameof(Index));
            }

            var plaza = repositorioPlaza.ObtenerPorId(estadia.ID_Plaza);
            if (plaza == null) return BadRequest("La plaza asociada no existe.");

            var tipoPlaza = repositorioTipoDePlaza.ObtenerPorId(plaza.ID_TipoDePlaza);
            if (tipoPlaza == null) return BadRequest("El tipo de plaza no existe.");

            DateTime fechaEgreso = DateTime.Now;
            TimeSpan duracion = fechaEgreso - estadia.Ingreso;

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
        public async Task<IActionResult> Cobrar(int ID_Estadia, IFormFile? comprobanteFile)
        {
            if (ID_Estadia <= 0) return BadRequest("La estadía no es válida.");

            var estadia = repositorioEstadia.ObtenerPorId(ID_Estadia);
            if (estadia == null) return NotFound();

            var pagoExistente = repositorioPago.ObtenerPorEstadiaId(ID_Estadia);
            if (pagoExistente != null)
            {
                TempData["ErrorMessage"] = "Esta estadía ya fue cobrada previamente.";
                return RedirectToAction(nameof(Index));
            }

            if (estadia.Egreso != null)
            {
                TempData["ErrorMessage"] = "No se puede cobrar una estadía que ya tiene salida.";
                return RedirectToAction("Index", "Estadia");
            }

            var plaza = repositorioPlaza.ObtenerPorId(estadia.ID_Plaza);
            if (plaza == null) return BadRequest("La plaza asociada no existe.");

            var tipoPlaza = repositorioTipoDePlaza.ObtenerPorId(plaza.ID_TipoDePlaza);
            if (tipoPlaza == null) return BadRequest("El tipo de plaza no existe.");

            if (tipoPlaza.PrecioXHora <= 0) return BadRequest("La tarifa debe ser mayor que cero.");

            DateTime fechaEgreso = DateTime.Now;
            if (estadia.Ingreso > fechaEgreso) return BadRequest("La fecha de ingreso está en el futuro.");

            TimeSpan duracion = fechaEgreso - estadia.Ingreso;
            double horas = Math.Max(1, Math.Ceiling(duracion.TotalHours));
            decimal montoCalculado = (decimal)horas * tipoPlaza.PrecioXHora;

            if (montoCalculado > 99999999.99m) return BadRequest("El importe supera el máximo permitido.");

            var pago = new Pago
            {
                ID_Estadia = estadia.ID_Estadia,
                FechaEmision = fechaEgreso,
                PrecioXHoraAplicado = tipoPlaza.PrecioXHora,
                MontoTotal = montoCalculado,
                ComprobanteUrl = null,
                Estadia = estadia
            };

            ViewBag.HorasCobradas = horas;
            ViewBag.DuracionReal = $"{(int)duracion.TotalHours}h {duracion.Minutes}m";

            if (comprobanteFile != null)
            {
                var resultadoGuardado = await GuardarComprobanteArchivo(comprobanteFile);
                if (!resultadoGuardado.Exito)
                {
                    ModelState.AddModelError("", resultadoGuardado.Error ?? "Error al procesar el archivo.");
                    return View(pago);
                }
                pago.ComprobanteUrl = resultadoGuardado.Url;
            }

            int idPago;
            try
            {
                idPago = repositorioPago.AltaConSalida(pago);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (MySql.Data.MySqlClient.MySqlException ex) when (ex.Number == 1062)
            {
                TempData["ErrorMessage"] = "Esta estadía ya tiene un pago registrado.";
                return RedirectToAction(nameof(Index));
            }

            if (idPago > 0)
            {
                TempData["SuccessMessage"] = $"Cobro N° {idPago} registrado exitosamente. La estadía fue finalizada.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Ocurrió un error al registrar el pago.");
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

        // =========================================================================
        // === ENDPOINTS WEB API REST (Autenticación JWT para Postman / Clientes) ===
        // =========================================================================

        // GET: api/pago
        [HttpGet("api/pago")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ApiObtenerTodos([FromQuery] string? busqueda, [FromQuery] int pagina = 1)
        {
            string filtro = (busqueda ?? "").Trim();
            var pagos = repositorioPago.ObtenerLista(pagina, TAMANO_PAGINA, filtro);
            foreach (var pago in pagos)
            {
                pago.Estadia = repositorioEstadia.ObtenerPorId(pago.ID_Estadia);
            }
            return Ok(pagos);
        }

        // GET: api/pago/5
        [HttpGet("api/pago/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ApiObtenerPorId(int id)
        {
            var pago = repositorioPago.ObtenerPorId(id);
            if (pago == null) return NotFound(new { mensaje = "Pago no encontrado." });

            pago.Estadia = repositorioEstadia.ObtenerPorId(pago.ID_Estadia);
            return Ok(pago);
        }

        // POST: api/pago/cobrar (Soporta multipart/form-data con archivo opcional)
        [HttpPost("api/pago/cobrar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ApiCobrar([FromForm] int idEstadia, [FromForm] IFormFile? comprobanteFile)
        {
            if (idEstadia <= 0) return BadRequest(new { mensaje = "La estadía especificada no es válida." });

            var estadia = repositorioEstadia.ObtenerPorId(idEstadia);
            if (estadia == null) return NotFound(new { mensaje = "Estadía no encontrada." });

            var pagoExistente = repositorioPago.ObtenerPorEstadiaId(idEstadia);
            if (pagoExistente != null) return BadRequest(new { mensaje = "Esta estadía ya fue cobrada previamente." });

            if (estadia.Egreso != null) return BadRequest(new { mensaje = "No se puede cobrar una estadía que ya tiene salida registrada." });

            var plaza = repositorioPlaza.ObtenerPorId(estadia.ID_Plaza);
            if (plaza == null) return BadRequest(new { mensaje = "La plaza asociada no existe." });

            var tipoPlaza = repositorioTipoDePlaza.ObtenerPorId(plaza.ID_TipoDePlaza);
            if (tipoPlaza == null) return BadRequest(new { mensaje = "El tipo de plaza no existe." });

            if (tipoPlaza.PrecioXHora <= 0)
            {
                return BadRequest(new
                {
                    mensaje = "La tarifa debe ser mayor que cero."
                });
            }

            DateTime fechaEgreso = DateTime.Now;

            if (estadia.Ingreso > fechaEgreso)
            {
                return BadRequest(new
                {
                    mensaje = "La fecha de ingreso está en el futuro."
                });
            }

            TimeSpan duracion = fechaEgreso - estadia.Ingreso;
            double horas = Math.Max(1, Math.Ceiling(duracion.TotalHours));
            decimal montoCalculado = (decimal)horas * tipoPlaza.PrecioXHora;

            if (montoCalculado > 99999999.99m)
            {
                return BadRequest(new
                {
                    mensaje = "El importe supera el máximo permitido."
                });
            }

            var pago = new Pago
            {
                ID_Estadia = estadia.ID_Estadia,
                FechaEmision = fechaEgreso,
                PrecioXHoraAplicado = tipoPlaza.PrecioXHora,
                MontoTotal = montoCalculado,
                ComprobanteUrl = null
            };

            if (comprobanteFile != null)
            {
                var res = await GuardarComprobanteArchivo(comprobanteFile);
                if (!res.Exito) return BadRequest(new { mensaje = res.Error });
                pago.ComprobanteUrl = res.Url;
            }

            try
            {
                int idPago = repositorioPago.AltaConSalida(pago);
                if (idPago > 0)
                {
                    pago.ID_Pago = idPago;
                    return CreatedAtAction(nameof(ApiObtenerPorId), new { id = idPago }, pago);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al procesar el pago.", detalle = ex.Message });
            }

            return StatusCode(500, new { mensaje = "No se pudo registrar el pago en la base de datos." });
        }

               // Los pagos registrados se conservan en el historial.
        [HttpDelete("api/pago/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Rol.Admin)]
        public IActionResult ApiEliminar(int id)
        {
            Response.Headers["Allow"] = "GET";

            return StatusCode(405, new
            {
                mensaje = "No se permite eliminar pagos registrados. Se conserva el historial de cobros."
            });
        }

        // Método auxiliar reutilizable para guardar y validar el comprobante
        private async Task<(bool Exito, string? Url, string? Error)> GuardarComprobanteArchivo(IFormFile archivo)
        {
            const long MAXIMO_BYTES = 5 * 1024 * 1024;
            if (archivo.Length == 0) return (false, null, "El archivo está vacío.");
            if (archivo.Length > MAXIMO_BYTES) return (false, null, "El comprobante no puede superar los 5 MB.");

            string extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (extension != ".pdf" && extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                return (false, null, "Solo se permiten archivos PDF, JPG y PNG.");

            using var contenido = new MemoryStream();
            await archivo.CopyToAsync(contenido);
            byte[] bytes = contenido.ToArray();

            bool esPdf = bytes.Length >= 5 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46 && bytes[4] == 0x2D;
            bool esJpg = bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
            bool esPng = bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 && bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;

            bool formatoValido = (extension == ".pdf" && esPdf) || ((extension == ".jpg" || extension == ".jpeg") && esJpg) || (extension == ".png" && esPng);
            if (!formatoValido) return (false, null, "El contenido del archivo no coincide con su extensión.");

            string uploadsFolder = Path.Combine(environment.WebRootPath, "uploads", "comprobantes");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.CreateNew))
            {
                await fileStream.WriteAsync(bytes.AsMemory());
            }

            return (true, $"/uploads/comprobantes/{uniqueFileName}", null);
        }
    }
}