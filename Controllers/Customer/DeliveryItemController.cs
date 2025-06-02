using Foodie.Models.customers;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Customer
{
    public class DeliveryItemController : Controller
    {
        private readonly IcustomerRepository _repository;

        public DeliveryItemController(IcustomerRepository repository)
        {
            _repository = repository;
        }

        public ActionResult AllDeliveryFood(int id)
        {
            var menuItem = _repository.GetInspireMenuItemById(id); // fetch from DB based on MenuId

            if (menuItem == null)
            {
                return BadRequest();
            }

            var viewModel = new MenuItemViewModel
            {
                MenuId = menuItem.MenuId,
                MenuName = menuItem.MenuName,
                MenuDescription = menuItem.MenuDescription,
                Amount = menuItem.Amount,
                MenuImg = menuItem.MenuImg,
                cuisine_id = menuItem.cuisine_id,
                RestaurantId = menuItem.RestaurantId,
                IsAvalable = menuItem.IsAvalable
            };

            return View("AllDeliveryFood", viewModel); // Ensure your view is named MenuDetails.cshtml
        }

        [HttpGet]
        public JsonResult GetAllCuisines()
        {
            var cuisines = _repository.GetAllCuisines();
            return Json(cuisines);
        }
    }
}
