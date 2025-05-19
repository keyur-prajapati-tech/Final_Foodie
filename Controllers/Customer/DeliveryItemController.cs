using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Customer
{
    public class DeliveryItemController : Controller
    {
        public IActionResult AllDeliveryFood()
        {
            return View();
        }
    }
}
