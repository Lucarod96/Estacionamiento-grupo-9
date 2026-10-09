using Microsoft.AspNetCore.Mvc;
using Estacionamiento_grupo_9.Models;
using System;
using System.Collections.Generic;

namespace Estacionamiento_grupo_9.Controllers
{
    public class ClienteController : Controller
    {
        private readonly IRepositorioCliente repositorioCliente;
        private const int TAMANO_PAGINA = 10;

        public ClienteController(IRepositorioCliente repositorioCliente)
        {
            this.repositorioCliente = repositorioCliente;
        }

        // GET: Cliente (con búsqueda y paginado por servidor)
        public IActionResult Index(string? busqueda, int pagina = 1)
        {
            if (pagina < 1) pagina = 1;

            IList<Cliente> clientes;
            int totalRegistros;

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                clientes = repositorioCliente.Buscar(busqueda);
                totalRegistros = clientes.Count;
                ViewBag.Busqueda = busqueda;
            }
            else
            {
                clientes = repositorioCliente.ObtenerLista(pagina, TAMANO_PAGINA);
                totalRegistros = repositorioCliente.ObtenerCantidad();
            }

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / TAMANO_PAGINA);
            ViewBag.TotalRegistros = totalRegistros;

            return View(clientes);
        }

        // GET: Cliente/Details/AA123CD
        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var cliente = repositorioCliente.ObtenerPorPatente(id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // GET: Cliente/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cliente/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                // Verificar si ya existe un cliente con esa patente
                var existente = repositorioCliente.ObtenerPorPatente(cliente.Patente);
                if (existente != null)
                {
                    ModelState.AddModelError("Patente", "Ya existe un vehículo registrado con esta patente.");
                    return View(cliente);
                }

                int res = repositorioCliente.Alta(cliente);
                if (res > 0)
                {
                    TempData["SuccessMessage"] = "Cliente/Vehículo registrado con éxito.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocurrió un error al intentar guardar el cliente.";
                }
            }
            return View(cliente);
        }

        // GET: Cliente/Edit/AA123CD
        public IActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var cliente = repositorioCliente.ObtenerPorPatente(id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // POST: Cliente/Edit/AA123CD
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, Cliente cliente)
        {
            if (id != cliente.Patente) return BadRequest();

            if (ModelState.IsValid)
            {
                int res = repositorioCliente.Modificacion(cliente);
                if (res > 0)
                {
                    TempData["SuccessMessage"] = "Datos del cliente actualizados correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudieron guardar los cambios.";
                }
            }
            return View(cliente);
        }

        // GET: Cliente/Delete/AA123CD
        public IActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var cliente = repositorioCliente.ObtenerPorPatente(id);
            if (cliente == null) return NotFound();

            return View(cliente);
        }

        // POST: Cliente/DeleteConfirmed/AA123CD
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            try
            {
                int res = repositorioCliente.Baja(id);
                if (res > 0)
                {
                    TempData["SuccessMessage"] = "Cliente eliminado del sistema.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar el cliente seleccionado.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "No se puede eliminar el cliente porque posee estadías o registros asociados en la base de datos.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}