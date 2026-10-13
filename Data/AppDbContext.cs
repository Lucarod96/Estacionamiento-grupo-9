using Estacionamiento_grupo_9.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Estacionamiento_grupo_9.Data
{
    // Este contexto SOLO maneja las tablas de Identity (Staff, roles, etc.).
    // El resto de las entidades siguen usando los repositorios con ADO.NET.
    public class AppDbContext : IdentityDbContext<Staff, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // La tabla de usuarios se llama "Staff" (como en el diagrama de clases)
            builder.Entity<Staff>().ToTable("Staff");
        }
    }
}
