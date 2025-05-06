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

        //public IActionResult AddToCartIteminfo()
        //{
        //     int customerId = Convert.ToInt32(HttpContext.Session.GetString("customer_id"));

        //     var cartItems = _repository.GetCartItems(customerId);
        //     return View(cartItems);
        //}

        [HttpGet]
        public IActionResult AddToCartIteminfo()
        {
            var customerIdString = HttpContext.Session.GetString("customer_id");

            if (string.IsNullOrEmpty(customerIdString))
            {
                // Session not available, redirect to login or show error
                return RedirectToAction("Locality", "Locality");
            }

            int customerId = Convert.ToInt32(customerIdString);

            var cartItems = _repository.GetCartItems(customerId);
            return View(cartItems);
        }


        [HttpPost]
        public IActionResult AddToCartIteminfo(int menuid, int quantity, decimal price)
        {
            int customerId = Convert.ToInt32(HttpContext.Session.GetString("customer_id"));

            // Get or create the cart
            var cart = _repository.GetOrCreateCart(customerId);

            // Add the item to the cart
            _repository.AddToCart(new Models.customers.tbl_cart_item
            {
                menu_id = menuid,
                cart_id = cart.cart_id,
                quantity = quantity,
                price = price
            });

            return View("AddToCartIteminfo");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Address(tbl_address address)
        {
            try
            {
                var sessionId = HttpContext.Session.GetString("customer_id");

                if (string.IsNullOrEmpty(sessionId))
                {
                    return Json(new { success = false, message = "Session expired. Please log in again." });
                }

                address.customer_id = Convert.ToInt32(sessionId);
                _repository.AddAddress(address);

                return Json(new { success = true, message = "Address added successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while adding the address: " + ex.Message });
            }
        }


        [HttpGet]
        public IActionResult GetAllStates()
        {
            var allstates = _repository.GetAllStates();
            return Json(allstates);
        }

        [HttpGet]
        public IActionResult GetDistricts(int stateId)
        {
            var districts = _repository.GetDistrictByStateId(stateId);
            return Json(districts);
        }

        [HttpGet]
        public IActionResult GetCities(int districtId)
        {
            var cities = _repository.GetCitiesByDistrictId(districtId);
            return Json(cities);
        }
    }
}
