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


        public IActionResult AllDeliveryFood(int id)
        {
            var items = _repository.GetInspireMenuItemById(id);
            return View(items);
        }
    }
}
