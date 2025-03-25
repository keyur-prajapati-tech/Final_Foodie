using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Customer
{
    public class displayCart : Controller
    {
        public IActionResult AddToCartIteminfo()
        {
            return View();
        }
    }
}
