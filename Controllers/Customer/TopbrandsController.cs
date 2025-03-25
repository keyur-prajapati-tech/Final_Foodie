using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Customer
{
    [Route("TBrands")]
    public class TopbrandsController : Controller
    {
        [Route("")]
        [Route("Brands")]
        public IActionResult Brands()
        {
            return View();
        }
    }
}
