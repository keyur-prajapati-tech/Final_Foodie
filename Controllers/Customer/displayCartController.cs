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

        [HttpGet]
        public IActionResult GetCustomerInfo(int id)
        {
            var customer = _repository.GetCustomerNameAndPhone(id);

            if(customer == null)
            {
                return NotFound("Customer Not Found");
            }
            return View(customer);
        }

        //Increasing quantity, Decreasing quantity, Removing item, Getting total
        [HttpPost]
        public IActionResult IncrementQuantity(int cartItemId,int customerId)
        {
            _repository.IncreaseQuantity(cartItemId);
            var total = _repository.GetCartTotal(customerId);
            return Json(new { success = true, total });
        }

        [HttpPost]
        public IActionResult DecrementQuantity(int cartItemId,int customerId)
        {
            _repository.DecreaseQuantity(cartItemId);
            var total = _repository.GetCartTotal(customerId);
            return Json(new { success = true, total });
        }

        [HttpPost]
        public IActionResult RemoveCartItem(int cartItemId,int customerId)
        {
            _repository.RemoveCartItem(cartItemId);
            var total = _repository.GetCartItems(customerId);
            return Json(new { success = true, total });
        }

        public IActionResult GetAllCoupons()
        {
            var coupons = _repository.GetAllCoupons();
            return PartialView("_CouponListPartial", coupons);
        }

        public IActionResult GetGrandTotalWithCoupon(int customer_id)
        {
            decimal grandTotal = _repository.CalculateGrandTotal(customer_id);

            var coupon = _repository.GetAutoApplicableCoupon(grandTotal);
            decimal discount = coupon != null ? coupon.discount : 0;
            decimal finalTotal = grandTotal - discount;

            return Json(new 
            { 
                success = true,
                grandTotal,
                discount,
                finalTotal,
                coupone_code = coupon?.coupone_code
            });
        }

        [HttpPost]
        public IActionResult SavePaymentAndPlaceOrder([FromBody] RazorPayViewModel model)
        {
            try
            {
                int orderId = _repository.PlaceOrder(
                    model.customer_id,
                    model.coupone_id,
                    model.AddressId,
                    model.PaymentModeId,
                    model.grandtotal,
                    model.discount_amount,
                    model.RazorpayPaymentId,
                    model.RazorpayOrderId
                );

                foreach (var item in model.Items)
                {
                    _repository.SaveOrderItem(orderId, item.menu_id, item.quantity, item.listprice, item.discount);
                }

                return Json(new { success = true, orderId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
