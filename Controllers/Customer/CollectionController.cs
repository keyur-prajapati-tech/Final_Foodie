﻿using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;
using Foodie.Models.Restaurant;

namespace Foodie.Controllers.Customer
{
    [Route("FCollection")]
    public class CollectionController : Controller
    {
        private readonly IcustomerRepository _repository;

        public CollectionController(IcustomerRepository repository)
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
            var data = _repository.GetAllRestaurants();
            return View(data);
        }

        [Route("Dinningout")]
        public IActionResult Dinningout()
        {
            return View();
        }
    }
}
