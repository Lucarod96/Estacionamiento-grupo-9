namespace Estacionamiento_grupo_9.Services
{
    public interface IAvatarService
    {
        // Devuelve un mensaje de error si el archivo no es válido, o null si está OK.
        string? Validar(IFormFile archivo);

        // Guarda el archivo en wwwroot/uploads/avatars y devuelve la URL relativa.
        Task<string> GuardarAsync(IFormFile archivo);

        // Borra el archivo físico de un avatar anterior (si existe).
        void Eliminar(string? avatarUrl);
    }
}
