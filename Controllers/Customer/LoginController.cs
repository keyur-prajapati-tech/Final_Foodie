using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Customer
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string phone)
        {
            TempData["DisplayHome"] = phone;

            return RedirectToAction("Delivery","FCollection");
            //return View();
        }
    }
}
