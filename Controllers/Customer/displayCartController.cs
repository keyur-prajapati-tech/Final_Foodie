using Foodie.Models;
using Foodie.Models.customers;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Razorpay.Api;

namespace Foodie.Controllers.Customer
{
    public class displayCartController : Controller
    {
        private readonly IcustomerRepository _repository;
        private readonly string _razorpaykey;
        private readonly string _razorpaysecret;

        public displayCartController(IcustomerRepository repository)
        {
            _repository = repository;
            _razorpaykey = "rzp_test_uIxZbgotqYKcAK";
            _razorpaysecret = "ZFN5H6NDvYchs04zgctFnWT3";
        }

        [HttpGet]
        public IActionResult AddToCartIteminfo()
        {
            var customerIdString = HttpContext.Session.GetString("UserId");

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
            int customerId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Address(tbl_address address)
        {
            try
            {
                var sessionId = HttpContext.Session.GetString("UserId");

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

            if (customer == null)
            {
                return NotFound("Customer Not Found");
            }
            return View(customer);
        }

        //Increasing quantity, Decreasing quantity, Removing item, Getting total
        [HttpPost]
        public IActionResult IncrementQuantity(int cartItemId, int customerId)
        {
            _repository.IncreaseQuantity(cartItemId);
            var total = _repository.GetCartTotal(customerId);
            return Json(new { success = true, total });
        }

        [HttpPost]
        public IActionResult DecrementQuantity(int cartItemId, int customerId)
        {
            _repository.DecreaseQuantity(cartItemId);
            var total = _repository.GetCartTotal(customerId);
            return Json(new { success = true, total });
        }

        [HttpPost]
        public IActionResult RemoveCartItem(int cartItemId, int customerId)
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
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

            int orderId = _repository.PlaceOrder(model);

            foreach (var item in model.Items)
            {
                _repository.SaveOrderItem(orderId, item);
            }

            return Json(new { success = true, orderId = orderId });
        }

        [HttpPost]
        public IActionResult InitiateOrder([FromBody] PaymentInitiateModel model)
        {
            int customerId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            try
            {
                var client = new RazorpayClient(_razorpaykey, _razorpaysecret);
                var options = new Dictionary<string, object>
                {
                    { "amount", model.amount * 100 }, // Amount in paise
                    { "currency", "INR" }
                };
                var order = client.Order.Create(options);
                string razorpayOrderId = order["id"].ToString();

                var orderItems = new List<tbl_order_items>();
                foreach (var item in model.OrderItems)
                {
                    orderItems.Add(new tbl_order_items
                    {
                        menu_id = item.menu_id,
                        quantity = item.quantity,
                        list_price = item.listprice,
                        discount = item.discount
                    });
                }

                var createdOrder = _repository.CreateOrder(customerId, model.amount, razorpayOrderId, orderItems, model.address_id);

                return Json(new { orderId = razorpayOrderId });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Success([FromBody] PaymentSuccessModel model)
        {
            try
            {
                var generatedSignature = GenerateSignature(model.razorpay_order_id, model.razorpay_payment_id, _razorpaysecret);

                if (generatedSignature == model.razorpay_signature)
                {
                    _repository.SavePayment(model.razorpay_order_id, model.razorpay_payment_id, model.amount, "Paid");
                    _repository.UpdateOrderStatus(model.razorpay_order_id, "Paid");

                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Payment verification failed" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private string GenerateSignature(string orderId, string paymentId, string secret)
        {
            var encoding = new System.Text.UTF8Encoding();
            byte[] keyBytes = encoding.GetBytes(secret);
            byte[] textBytes = encoding.GetBytes($"{orderId}|{paymentId}");

            using (var hmac = new System.Security.Cryptography.HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(textBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
