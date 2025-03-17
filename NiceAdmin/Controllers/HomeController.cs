using Microsoft.AspNetCore.Mvc;

namespace NiceAdmin.Controllers
{
    [CheckAccess]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
