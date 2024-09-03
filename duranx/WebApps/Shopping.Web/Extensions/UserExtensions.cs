using System.Security.Claims;

namespace Shopping.Web.Extensions
{
    public static class UserExtensions
    {
        public static Guid GetId(this ClaimsPrincipal User)
        {
            return new Guid(User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.NameIdentifier))!.Value);
        }
    }
}
