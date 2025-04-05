using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Restaurant
{
    public class OrderHistoryController : Controller
    {
        public IActionResult OrdHistory()
        {
            return View();
        }

        public IActionResult offers()
        {
            return View();
        }

        public IActionResult reports()
        {
            return View();
        }
    }
}
