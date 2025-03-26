using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Customer
{
    public class AddressController : Controller
    {
        public IActionResult LocationGet()
        {
            return View();
        }
    }
}
