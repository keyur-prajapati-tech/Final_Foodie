using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Customer
{
    public class RestaurantdeatilController : Controller
    {
        private readonly IcustomerRepository _repository;

        public RestaurantdeatilController(IcustomerRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Restuarantinfo()
        {
            var items = _repository.GetAllMenuItems();
            return View(items);
        }

        [HttpGet]
        public IActionResult GetMenuItemById(int id)
        {
            var item = _repository.GetMenuItemById(id);
            if (item == null)
            {
                return NotFound();
            }

            return Json(new
            {
                menu_name = item.menu_name,
                menu_descripation = item.menu_descripation,
                amount = item.amount,
                menu_img = Convert.ToBase64String(item.menu_img)
            });
        }
    }
}
