using Microsoft.AspNetCore.Mvc;

namespace Obligatorio_Cliente.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
