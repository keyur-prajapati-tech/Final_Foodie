using Foodie.Models.customers;
using Foodie.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
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
            var recommendedRestaurants = _repository.GetRecommendedRestaurants();
            return View(recommendedRestaurants);
            //var menuItem = _repository.GetInspireMenuItemById(id); // fetch from DB based on MenuId

            //if (menuItem == null)
            //{
            //    return BadRequest();
            //}

            //var viewModel = new MenuItemViewModel
            //{
            //    MenuId = menuItem.MenuId,
            //    MenuName = menuItem.MenuName,
            //    MenuDescription = menuItem.MenuDescription,
            //    Amount = menuItem.Amount,
            //    MenuImg = menuItem.MenuImg,
            //    cuisine_id = menuItem.cuisine_id,
            //    RestaurantId = menuItem.RestaurantId,
            //    IsAvalable = menuItem.IsAvalable
            //};

            //return View("AllDeliveryFood", viewModel); // Ensure your view is named MenuDetails.cshtml
        }

        [HttpGet]
        public JsonResult GetAllCuisines()
        {   
            var cuisines = _repository.GetAllCuisines();
            return Json(cuisines);
        }

        [HttpGet]
        public JsonResult GetByCuisine(int cuisineId)
        {
            var restaurants = _repository.GetRestaurantsByCuisine(cuisineId);
            return Json(restaurants);
        }

        [HttpGet]
        public JsonResult Search(string term, string pincode = null)
        {
            var results = _repository.SearchRestaurants(term);
            return Json(results);
        }

        [HttpGet("all")]
        public IActionResult GetAllMenuItems()
        {
            try
            {
                var menuItems =  _repository.GetAllMenuItems();
                return Ok(menuItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("restaurants")]
        public IActionResult GetAllRestaurants()
        {
            try
            {
                var restaurants = _repository.GetAllRestaurants();
                return Ok(restaurants);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("by-restaurant/{restaurantId}")]
        public IActionResult GetMenuByRestaurant(int restaurantId)
        {
            try
            {
                var menuItems = _repository.GetMenuItemsByRestaurant(restaurantId);
                return Ok(menuItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
