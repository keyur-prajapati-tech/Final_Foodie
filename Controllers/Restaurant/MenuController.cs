using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Restaurant
{
    [Route("Menu")]
    public class MenuController : Controller
    {
        [Route("Add")]
        public IActionResult AddMenu()
        {
            return View();
        }

        [Route("Inventory")]
        public IActionResult Inventory()
        {
            return View();
        }

        [Route("InvDishes")]
        public IActionResult Dishes()
        {
            return View();
        }

        [Route("InvAddOns")]
        public IActionResult AddOns()
        {
            return View();
        }
    }
}
