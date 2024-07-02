using Microsoft.AspNetCore.Mvc;

namespace SIA.Controllers
{
    public class DashBoardController : Controller
    {
        // GET: Dashboard
        public IActionResult Index()
        {
            return View();
        }

    }
}