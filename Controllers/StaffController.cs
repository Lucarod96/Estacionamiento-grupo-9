using Estacionamiento_grupo_9.Models;
using Estacionamiento_grupo_9.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Estacionamiento_grupo_9.Controllers
{
    // Todo el controlador requiere estar logueado, salvo las acciones con [AllowAnonymous].
    [Authorize]
    public class StaffController : Controller
    {
        private readonly UserManager<Staff> userManager;
        private readonly SignInManager<Staff> signInManager;
        private readonly IAvatarService avatarService;
        private const int TAMANO_PAGINA = 10;

        public StaffController(
            UserManager<Staff> userManager,
            SignInManager<Staff> signInManager,
            IAvatarService avatarService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.avatarService = avatarService;
        }

        // ==========================================
        // === VISTAS Y RUTAS MVC (Autenticación Cookie) ===
        // ==========================================

        // ---------------------------------------------------------------
        // LOGIN / LOGOUT
        // ---------------------------------------------------------------

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid) return View(model);

            var resultado = await signInManager.PasswordSignInAsync(
                model.Usuario.Trim(), model.Password, model.Recordarme, lockoutOnFailure: true);

            if (resultado.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (resultado.IsLockedOut)
                ModelState.AddModelError(string.Empty, "Demasiados intentos fallidos. Probá de nuevo en unos minutos.");
            else
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ---------------------------------------------------------------
        // PERFIL (cualquier usuario logueado: ver datos y cambiar su avatar)
        // ---------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            return View(await ConstruirPerfilAsync(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(PerfilViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            if (model.AvatarFile == null || model.AvatarFile.Length == 0)
            {
                ModelState.AddModelError(nameof(model.AvatarFile), "Elegí una imagen.");
            }
            else
            {
                var error = avatarService.Validar(model.AvatarFile);
                if (error != null) ModelState.AddModelError(nameof(model.AvatarFile), error);
            }

            if (!ModelState.IsValid)
                return View(await ConstruirPerfilAsync(user));

            var anterior = user.AvatarUrl;
            user.AvatarUrl = await avatarService.GuardarAsync(model.AvatarFile!);

            var resultado = await userManager.UpdateAsync(user);
            if (!resultado.Succeeded)
            {
                avatarService.Eliminar(user.AvatarUrl);
                TempData["ErrorMessage"] = "No se pudo actualizar el avatar.";
                return RedirectToAction(nameof(Perfil));
            }

            avatarService.Eliminar(anterior);
            await signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Avatar actualizado correctamente.";
            return RedirectToAction(nameof(Perfil));
        }

        // ---------------------------------------------------------------
        // ABM de Staff (solo Admin)
        // ---------------------------------------------------------------

        [Authorize(Roles = Rol.Admin)]
        public async Task<IActionResult> Index(int pagina = 1)
        {
            if (pagina < 1) pagina = 1;

            int total = await userManager.Users.CountAsync();
            int totalPaginas = Math.Max(1, (int)Math.Ceiling((double)total / TAMANO_PAGINA));
            if (pagina > totalPaginas) pagina = totalPaginas;

            var usuarios = await userManager.Users
                .OrderBy(u => u.UserName)
                .Skip((pagina - 1) * TAMANO_PAGINA)
                .Take(TAMANO_PAGINA)
                .ToListAsync();

            var items = new List<StaffListItem>();
            foreach (var u in usuarios)
            {
                items.Add(await ConstruirItemAsync(u));
            }

            return View(new StaffIndexViewModel
            {
                Items = items,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = total
            });
        }

        [Authorize(Roles = Rol.Admin)]
        [HttpGet]
        public IActionResult Create()
        {
            CargarRoles(Rol.Operador);
            return View(new StaffCreateViewModel());
        }

        [Authorize(Roles = Rol.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffCreateViewModel model)
        {
            CargarRoles(model.Rol);

            if (!Rol.Todos.Contains(model.Rol))
                ModelState.AddModelError(nameof(model.Rol), "El rol seleccionado no es válido.");

            bool hayAvatar = model.AvatarFile != null && model.AvatarFile.Length > 0;
            if (hayAvatar)
            {
                var error = avatarService.Validar(model.AvatarFile!);
                if (error != null) ModelState.AddModelError(nameof(model.AvatarFile), error);
            }

            if (!ModelState.IsValid) return View(model);

            var user = new Staff
            {
                UserName = model.UserName.Trim(),
                Email = model.Email.Trim(),
                EmailConfirmed = true
            };

            var creado = await userManager.CreateAsync(user, model.Password);
            if (!creado.Succeeded)
            {
                foreach (var e in creado.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            var rolAsignado = await userManager.AddToRoleAsync(user, model.Rol);
            if (!rolAsignado.Succeeded)
            {
                await userManager.DeleteAsync(user);
                foreach (var e in rolAsignado.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            if (hayAvatar)
            {
                user.AvatarUrl = await avatarService.GuardarAsync(model.AvatarFile!);
                await userManager.UpdateAsync(user);
            }

            TempData["SuccessMessage"] = $"Usuario '{user.UserName}' creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = Rol.Admin)]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            var roles = await userManager.GetRolesAsync(user);
            var rolActual = roles.FirstOrDefault() ?? Rol.Operador;
            CargarRoles(rolActual);

            return View(new StaffEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email ?? string.Empty,
                Rol = rolActual,
                AvatarUrl = user.AvatarUrl
            });
        }

        [Authorize(Roles = Rol.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StaffEditViewModel model)
        {
            if (id != model.Id) return BadRequest();

            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            model.UserName = user.UserName;
            model.AvatarUrl = user.AvatarUrl;
            CargarRoles(model.Rol);

            if (!Rol.Todos.Contains(model.Rol))
                ModelState.AddModelError(nameof(model.Rol), "El rol seleccionado no es válido.");

            if (user.Id == IdActual() && model.Rol != Rol.Admin)
                ModelState.AddModelError(nameof(model.Rol), "No podés quitarte el rol Admin a vos mismo.");

            bool hayAvatar = model.AvatarFile != null && model.AvatarFile.Length > 0;
            if (hayAvatar)
            {
                var error = avatarService.Validar(model.AvatarFile!);
                if (error != null) ModelState.AddModelError(nameof(model.AvatarFile), error);
            }

            if (!ModelState.IsValid) return View(model);

            var nuevoEmail = model.Email.Trim();
            if (!string.Equals(user.Email, nuevoEmail, StringComparison.OrdinalIgnoreCase))
            {
                var resEmail = await userManager.SetEmailAsync(user, nuevoEmail);
                if (!resEmail.Succeeded)
                {
                    foreach (var e in resEmail.Errors) ModelState.AddModelError(nameof(model.Email), e.Description);
                    return View(model);
                }
            }

            var rolesActuales = await userManager.GetRolesAsync(user);
            if (!rolesActuales.Contains(model.Rol) || rolesActuales.Count > 1)
            {
                await userManager.RemoveFromRolesAsync(user, rolesActuales);
                await userManager.AddToRoleAsync(user, model.Rol);
            }

            if (hayAvatar)
            {
                var anterior = user.AvatarUrl;
                user.AvatarUrl = await avatarService.GuardarAsync(model.AvatarFile!);

                var resAvatar = await userManager.UpdateAsync(user);
                if (resAvatar.Succeeded)
                {
                    avatarService.Eliminar(anterior);
                }
                else
                {
                    avatarService.Eliminar(user.AvatarUrl);
                    TempData["ErrorMessage"] = "Se guardaron los datos, pero no se pudo actualizar el avatar.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (user.Id == IdActual())
                await signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = $"Usuario '{user.UserName}' actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = Rol.Admin)]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            return View(await ConstruirItemAsync(user));
        }

        [Authorize(Roles = Rol.Admin)]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            if (user.Id == IdActual())
            {
                TempData["ErrorMessage"] = "No podés eliminar tu propio usuario.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var resultado = await userManager.DeleteAsync(user);
                if (resultado.Succeeded)
                {
                    avatarService.Eliminar(user.AvatarUrl);
                    TempData["SuccessMessage"] = $"Usuario '{user.UserName}' eliminado.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo eliminar el usuario.";
                }
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "No se puede eliminar el usuario porque tiene estadías registradas.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================================
        // === ENDPOINTS WEB API REST (Autenticación JWT para Postman / Clientes) ===
        // =========================================================================

        // GET: api/staff/perfil (Perfil del usuario autenticado)
        [HttpGet("api/staff/perfil")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ApiPerfil()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { mensaje = "Token inválido." });

            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { mensaje = "Usuario no encontrado." });

            var perfil = await ConstruirPerfilAsync(user);
            return Ok(perfil);
        }

        // POST: api/staff/perfil/avatar (Actualizar avatar del usuario autenticado)
        [HttpPost("api/staff/perfil/avatar")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ApiActualizarAvatar([FromForm] IFormFile? avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0)
                return BadRequest(new { mensaje = "Debe adjuntar una imagen válida." });

            var error = avatarService.Validar(avatarFile);
            if (error != null) return BadRequest(new { mensaje = error });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userId ?? string.Empty);
            if (user == null) return NotFound(new { mensaje = "Usuario no encontrado." });

            var anterior = user.AvatarUrl;
            user.AvatarUrl = await avatarService.GuardarAsync(avatarFile);

            var resultado = await userManager.UpdateAsync(user);
            if (!resultado.Succeeded)
            {
                avatarService.Eliminar(user.AvatarUrl);
                return StatusCode(500, new { mensaje = "No se pudo actualizar el avatar en la base de datos." });
            }

            avatarService.Eliminar(anterior);
            return Ok(new { mensaje = "Avatar actualizado correctamente.", avatarUrl = user.AvatarUrl });
        }

        // GET: api/staff (Listar personal - Restringido a Administradores)
        [HttpGet("api/staff")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Rol.Admin)]
        public async Task<IActionResult> ApiObtenerTodos([FromQuery] int pagina = 1)
        {
            if (pagina < 1) pagina = 1;

            int total = await userManager.Users.CountAsync();
            int totalPaginas = Math.Max(1, (int)Math.Ceiling((double)total / TAMANO_PAGINA));
            if (pagina > totalPaginas) pagina = totalPaginas;

            var usuarios = await userManager.Users
                .OrderBy(u => u.UserName)
                .Skip((pagina - 1) * TAMANO_PAGINA)
                .Take(TAMANO_PAGINA)
                .ToListAsync();

            var items = new List<StaffListItem>();
            foreach (var u in usuarios)
            {
                items.Add(await ConstruirItemAsync(u));
            }

            return Ok(new { items, pagina, totalPaginas, totalRegistros = total });
        }

        // GET: api/staff/5 (Obtener detalle de personal por ID - Restringido a Administradores)
        [HttpGet("api/staff/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Rol.Admin)]
        public async Task<IActionResult> ApiObtenerPorId(int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound(new { mensaje = "Usuario no encontrado." });

            var item = await ConstruirItemAsync(user);
            return Ok(item);
        }

        // DELETE: api/staff/5 (Eliminar personal - Restringido a Administradores)
        [HttpDelete("api/staff/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Rol.Admin)]
        public async Task<IActionResult> ApiEliminar(int id)
        {
            var user = await userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound(new { mensaje = "Usuario no encontrado." });

            var currentUserId = IdActual();
            if (currentUserId.HasValue && user.Id == currentUserId.Value)
            {
                return BadRequest(new { mensaje = "No podés eliminar tu propio usuario." });
            }

            try
            {
                var resultado = await userManager.DeleteAsync(user);
                if (resultado.Succeeded)
                {
                    avatarService.Eliminar(user.AvatarUrl);
                    return Ok(new { mensaje = $"Usuario '{user.UserName}' eliminado correctamente." });
                }
                return StatusCode(500, new { mensaje = "No se pudo eliminar el usuario." });
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { mensaje = "No se puede eliminar el usuario porque posee registros o estadías asociadas." });
            }
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private int? IdActual()
        {
            return int.TryParse(userManager.GetUserId(User), out var id) ? id : null;
        }

        private void CargarRoles(string? seleccionado)
        {
            ViewBag.Roles = new SelectList(Rol.Todos, seleccionado);
        }

        private async Task<PerfilViewModel> ConstruirPerfilAsync(Staff user)
        {
            var roles = await userManager.GetRolesAsync(user);
            return new PerfilViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                Rol = string.Join(", ", roles),
                AvatarUrl = user.AvatarUrl
            };
        }

        private async Task<StaffListItem> ConstruirItemAsync(Staff user)
        {
            var roles = await userManager.GetRolesAsync(user);
            return new StaffListItem
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Rol = string.Join(", ", roles),
                AvatarUrl = user.AvatarUrl
            };
        }
    }
}