using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.DeliveryPartner
{
    [Route("deliverymoney")]
    public class DeliveryMoney : Controller
    {
        [Route("m/wallet")]
        public IActionResult Wallet()
        {
            return View();
        }

        [Route("m/earn")]
        public IActionResult Earnings()
        {
            return View();
        }

        [Route("m/order")]
        public IActionResult Assigned_Orders()
        {
            return View();
        }
    }
}
