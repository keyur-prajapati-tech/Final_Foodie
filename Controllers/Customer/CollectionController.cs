using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;
using Foodie.Models.Restaurant;

namespace Foodie.Controllers.Customer
{
    [Route("FCollection")]
    public class CollectionController : Controller
    {
        private readonly IRestaurantRepository _repository;

        public CollectionController(IRestaurantRepository repository)
        {
            _repository = repository;
        }

        [Route("")]
        [Route("FoodCollection")]
        public IActionResult Foodcollection()
        {
            return View();  
        }

        [Route("Delivery")]
        public IActionResult Delivery()
        {
           // var data = _repository.GetAllRestaurants();
            return View();
        }

        [Route("Dinningout")]
        public IActionResult Dinningout()
        {
            return View();
        }
    }
}
