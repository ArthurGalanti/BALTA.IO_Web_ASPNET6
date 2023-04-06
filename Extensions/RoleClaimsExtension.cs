using System.Security.Claims;
using BlogAPI.Models;

namespace BlogAPI.Extensions;

public static class RoleClaimsExtension
{
    public static IEnumerable<Claim> GetClaims(this User user)
    {
        var result = new List<Claim>
        {
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.GivenName, user.Name)
        };
        result.AddRange(
            user.Roles.Select(role=> new Claim(ClaimTypes.Role, role.Slug))
            );
        return result;
    }
}