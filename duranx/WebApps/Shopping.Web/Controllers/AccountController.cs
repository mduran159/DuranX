using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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

            // Redirige al servidor OpenIddict para la autenticación
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, 
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            // Redirige al servidor OpenIddict para el logout
            var callbackUrl = "/Home";

            // Cerrar sesión en la aplicación y redirigir al servidor de OpenIddict para cerrar sesión allí también
            return SignOut(new AuthenticationProperties { RedirectUri = callbackUrl },
                           OpenIdConnectDefaults.AuthenticationScheme,
                           CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
