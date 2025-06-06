﻿using Foodie.Repositories;
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

        public IActionResult Restuarantinfo(int id)
        {
            var items = _repository.GetMenuItemsByRestaurant(id);
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

        public IActionResult Details(int id,int? cuisineId)
        {
            var model = _repository.GetRestaurantMenu(id, cuisineId);
            return View(model);
        }

        public FileContentResult ShowImage(byte[] imageBytes)
        {
            return File(imageBytes, "image/jpeg");
        }

        [HttpGet]
        public IActionResult GetMenuItemsByCuisine(int restaurantId, int cuisineId)
        {
            var items = _repository.GetMenuItemsByRestaurantAndCuisine(restaurantId, cuisineId);
            return PartialView("_MenuItemsPartial", items);
        }   
    }
}
