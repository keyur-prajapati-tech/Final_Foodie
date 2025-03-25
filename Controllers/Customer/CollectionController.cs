using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Customer
{
    [Route("FCollection")]
    public class CollectionController : Controller
    {
        [Route("")]
        [Route("FoodCollection")]
        public IActionResult Foodcollection()
        {
            return View();  
        }

        [Route("Delivery")]
        public IActionResult Delivery()
        {
            return View();
        }

        [Route("Dinningout")]
        public IActionResult Dinningout()
        {
            return View();
        }
    }
}
