using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Restaurant
{
    [Route("AddRestaurant")]
    public class AddrestaurantController : Controller
    {
        [Route("Registerres")]
        public IActionResult Registerres()
        {
            return View();
        }

        [Route("ResDocument")]
        public IActionResult ResDocument()
        {
            return View();
        }

        [Route("MenuItems")]
        public IActionResult MenuItems()
        {
            return View();
        }

        [Route("home")]
        public IActionResult home()
        {
            return View();
        }
    }
}
