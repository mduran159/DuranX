using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OpeniddictServer.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

            if (User.Identity.IsAuthenticated)
            {
                ViewData["Message"] = "You are logged in.";
            }
            else
            {
                ViewData["Message"] = "You are not logged in.";
            }
        }
    }
}
