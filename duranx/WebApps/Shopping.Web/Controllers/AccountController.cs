using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;

namespace Shopping.Web.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            // Valida que la returnUrl sea una URL válida dentro de la aplicación
            if (!Url.IsLocalUrl(returnUrl))
            {
                returnUrl = Url.Content("~/"); // Redirige a la página de inicio si la returnUrl no es válida
            }

            var properties = new AuthenticationProperties
            {
                RedirectUri = returnUrl // Define a dónde redirigir después de la autenticación
            };

            // Redirige al servidor OpenIddict para la autenticación
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }
    }

}
