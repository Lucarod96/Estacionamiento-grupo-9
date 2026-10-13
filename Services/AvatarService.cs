namespace Estacionamiento_grupo_9.Services
{
    public class AvatarService : IAvatarService
    {
        private const long TamanoMaximo = 2 * 1024 * 1024; // 2 MB
        private const string CarpetaUrl = "/uploads/avatars/";
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly IWebHostEnvironment env;

        public AvatarService(IWebHostEnvironment env)
        {
            this.env = env;
        }

        public string? Validar(IFormFile archivo)
        {
            if (archivo.Length == 0)
                return "El archivo del avatar está vacío.";

            if (archivo.Length > TamanoMaximo)
                return "El avatar no puede superar los 2 MB.";

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
                return "Formato no permitido. Usá JPG, PNG o WEBP.";

            if (!ContenidoEsImagen(archivo))
                return "El archivo no parece ser una imagen válida.";

            return null;
        }

        public async Task<string> GuardarAsync(IFormFile archivo)
        {
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            var carpeta = CarpetaFisica();
            Directory.CreateDirectory(carpeta);

            // Nombre generado por nosotros: nunca usamos el nombre que manda el cliente
            var nombre = $"{Guid.NewGuid():N}{extension}";
            var ruta = Path.Combine(carpeta, nombre);

            using (var stream = new FileStream(ruta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return CarpetaUrl + nombre;
        }

        public void Eliminar(string? avatarUrl)
        {
            if (string.IsNullOrWhiteSpace(avatarUrl) || !avatarUrl.StartsWith(CarpetaUrl))
                return;

            var nombre = Path.GetFileName(avatarUrl);
            var ruta = Path.Combine(CarpetaFisica(), nombre);
            if (File.Exists(ruta))
            {
                File.Delete(ruta);
            }
        }

        private string CarpetaFisica() => Path.Combine(env.WebRootPath, "uploads", "avatars");

        // Chequea los primeros bytes (firma) para no confiar solo en la extensión
        private static bool ContenidoEsImagen(IFormFile archivo)
        {
            var cabecera = new byte[12];
            int leidos;
            using (var stream = archivo.OpenReadStream())
            {
                leidos = stream.Read(cabecera, 0, cabecera.Length);
            }

            if (leidos < 12) return false;

            bool esJpeg = cabecera[0] == 0xFF && cabecera[1] == 0xD8 && cabecera[2] == 0xFF;
            bool esPng = cabecera[0] == 0x89 && cabecera[1] == 0x50 && cabecera[2] == 0x4E && cabecera[3] == 0x47;
            bool esWebp = cabecera[0] == 0x52 && cabecera[1] == 0x49 && cabecera[2] == 0x46 && cabecera[3] == 0x46
                       && cabecera[8] == 0x57 && cabecera[9] == 0x45 && cabecera[10] == 0x42 && cabecera[11] == 0x50;

            return esJpeg || esPng || esWebp;
        }
    }
}
