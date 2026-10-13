using Microsoft.AspNetCore.Mvc;

namespace Estacionamiento_grupo_9.Services
{
    public static class AvatarHelper
    {
        // Uso en las vistas: <img src="@Url.Avatar(Model.AvatarUrl)" />
        public static string Avatar(this IUrlHelper url, string? avatarUrl)
        {
            return string.IsNullOrWhiteSpace(avatarUrl)
                ? url.Content("~/img/default-avatar.svg")
                : avatarUrl;
        }
    }
}
