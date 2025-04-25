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

        public IActionResult GetItemInfo(int id)
        {
            var item = _repository.GetMenuItemById(id);
            if (item == null)
            {
                return NotFound();
            }
            return PartialView("_MenuItemModalPartial", item);
        }
    }
}
