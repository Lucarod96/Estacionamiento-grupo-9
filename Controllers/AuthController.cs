using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Estacionamiento_grupo_9.DTOs;
using Estacionamiento_grupo_9.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Estacionamiento_grupo_9.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Staff> _userManager;
        private readonly SignInManager<Staff> _signInManager;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<Staff> userManager,
            SignInManager<Staff> signInManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { mensaje = "Debe ingresar usuario/email y contraseña." });
            }

            // 1. Buscar usuario por Email o por UserName
            var usuario = await _userManager.FindByEmailAsync(dto.Email) 
                       ?? await _userManager.FindByNameAsync(dto.Email);

            if (usuario == null)
            {
                return Unauthorized(new { mensaje = "Usuario no encontrado." });
            }

            // 2. Validar contraseña con SignInManager (misma validación que la web)
            var resultado = await _signInManager.CheckPasswordSignInAsync(usuario, dto.Password, false);
            if (!resultado.Succeeded)
            {
                return Unauthorized(new { mensaje = "Contraseña incorrecta." });
            }

            // 3. Obtener Rol
            var roles = await _userManager.GetRolesAsync(usuario);
            var rolPrincipal = roles.FirstOrDefault() ?? "Operador";

            // 4. Generar Token JWT
            var tokenString = GenerarJwtToken(usuario, rolPrincipal);

            return Ok(new
            {
                token = tokenString,
                email = usuario.Email,
                nombre = usuario.UserName ?? usuario.Email,
                rol = rolPrincipal
            });
        }

        private string GenerarJwtToken(Staff usuario, string rol)
        {
            var jwtKey = _config["Jwt:Key"] ?? "ClaveSecretaSuperSeguraYDeAlMenos32Caracteres!";
            var jwtIssuer = _config["Jwt:Issuer"];
            var jwtAudience = _config["Jwt:Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, usuario.UserName ?? usuario.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, rol)
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}