using Foodie.Models.customers;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Foodie.Controllers.Customer
{
    public class displayCartController : Controller
    {
        private readonly IcustomerRepository _repository;

        public displayCartController(IcustomerRepository repository)
        {
            _repository = repository;
        }

        //[HttpGet]
        //public IActionResult MenuItemDetails(int menuId)
        //{
        //    var item = _repository.GetMenuItem(menuId);
        //    if(item == null)
        //    {
        //        return NotFound();
        //    }
        //    return PartialView("_MenuItemModalPartial",item);
        //}

       public IActionResult AddToCartIteminfo()
       {
            int customerId = Convert.ToInt32(HttpContext.Session.GetString("customerId"));

            var cartItems = _repository.GetCartItems(customerId);
            return View(cartItems);
       }
       
        //public IActionResult MyCart()
        //{
        //    int customerId = int.Parse(HttpContext.Session.GetString("customer_id"));
        //    var cartItems = _repository.GetCartItems(customerId);
        //    if (cartItems != null && cartItems.Count > 0)
        //    {
        //        return View(cartItems);
        //    }
        //    else
        //    {
        //        return View("EmptyCart");
        //    }
        //}

        public IActionResult Address()
        {
            var states = _repository.GetAllStates();
            ViewBag.States = new SelectList(states, "staed_id","state_name");
            return View();
        }

        [HttpGet]
        public JsonResult GetDistricts(int stateId)
        {
            var districts = _repository.GetDistrictByStateId(stateId);
            return Json(districts);
        }

        [HttpGet]
        public JsonResult GetCities(int districtId)
        {
            var cities = _repository.GetCitiesByDistrictId(districtId);
            return Json(cities);
        }

        //public IActionResult Checkout()
        //{
        //    int customerId = int.Parse(HttpContext.Session.GetString("CustomerId"));
        //    _repository.Placeorder(customerId);
        //    return RedirectToAction("OrderSuccess");
        //}
    }
}
