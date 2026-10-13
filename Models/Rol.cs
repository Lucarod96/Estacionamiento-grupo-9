namespace Estacionamiento_grupo_9.Models
{
    // Nombres de roles centralizados para usar en [Authorize(Roles = Rol.Admin)]
    public static class Rol
    {
        public const string Admin = "Admin";
        public const string Operador = "Operador";

        public static readonly string[] Todos = { Admin, Operador };
    }
}
