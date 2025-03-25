using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Restaurant
{
    [Route("Restaurant")]
    public class OrderNotificationController : Controller
    {
        [Route("OrderNoti")]
        public IActionResult OrderNoti()
        {
            return View();
        }

        [Route("OrderReady")]
        public IActionResult OrderReady()
        {
            return View();
        }

        [Route("OrderPickedUp")]
        public IActionResult OrderPickedup()
        {
            return View();
        }
    }
}
