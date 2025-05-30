using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Customer
{
    [Route("TBrands")]
    public class TopbrandsController : Controller
    {
        private readonly IcustomerRepository _repository;

        public TopbrandsController(IcustomerRepository repository)
        {
            _repository = repository;
        }

        [Route("")]
        [Route("Brands")]
        public IActionResult Brands()
        {
            var topbrands = _repository.GetTopRestaurant(5);
            return View(topbrands);
        }

        [Route("BrandDetails")]
        public IActionResult BrandDetails(string name)
        {
            var brand = _repository.GetRestaurantByName(name);
            if (brand == null)
            {
                return NotFound();
            }
            // Get menu items for this brand
            var menuItems = _repository.GetMenuItemsByRestaurantdetails(brand.RestaurantId);

            ViewBag.MenuItems = menuItems;
            return View(brand);
        }

        [HttpGet("GetBrandDetails")]
        public IActionResult GetBrandDetails(string brandName)
        {
            var brand = _repository.GetRestaurantByName(brandName);
            if (brand == null)
            {
                return Json(new { success = false, message = "Brand not found" });
            }
            return Json(new { success = true, data = brand });
        }
    }
}
