using System.Security.Claims;
using Estacionamiento_grupo_9.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Estacionamiento_grupo_9.Services
{
    // Agrega el avatar como claim para poder mostrarlo en el navbar
    // sin consultar la base de datos en cada request.
    public class StaffClaimsPrincipalFactory : UserClaimsPrincipalFactory<Staff, IdentityRole<int>>
    {
        public const string AvatarClaim = "AvatarUrl";

        public StaffClaimsPrincipalFactory(
            UserManager<Staff> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Staff user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim(AvatarClaim, user.AvatarUrl ?? string.Empty));
            return identity;
        }
    }
}
