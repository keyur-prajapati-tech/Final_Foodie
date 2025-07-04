﻿using Foodie.Models;
using Foodie.Models.customers;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Razorpay.Api;

namespace Foodie.Controllers.Customer
{
    public class displayCartController : Controller
    {
        private readonly IcustomerRepository _repository;
        private readonly string _razorpaykey;
        private readonly string _razorpaysecret;
        private readonly ILogger<displayCartController> _logger;

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

        public IActionResult GetAllCoupons()
        {
            //var coupons = _repository.GetAllCoupons();

            //return PartialView("_CouponListPartial", coupons);
            try
            {
                var coupons = _repository.GetAllCoupons();
                var grandTotal = GetGrandTotal();

                var couponViewModels = coupons.Select(c => new CouponeViewModel
                {
                    coupone_code = c.coupone_code,
                    description = c.description,
                    discount = c.discount,
                    MinimumOrderAmount = c.application_order_amount,
                    ExpiryDate = c.expiry_date,
                    IsApplicable = grandTotal >= c.application_order_amount,
                    ApplicabilityMessage = grandTotal >= c.application_order_amount
                        ? "Applicable to your order"
                        : $"Add ₹{(c.application_order_amount - grandTotal):0.00} more to use this coupon"
                }).ToList();

                return PartialView("_CouponListPartial", coupons);
            }
            catch (Exception ex)
            {
                // Log error
                return PartialView("_CouponListPartial", new List<CouponeViewModel>());
            }
        }

        private decimal GetGrandTotal()
        {
            try
            {
                var customerId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(customerId))
                    return 0;

                var cartItems = _repository.GetCartItems(int.Parse(customerId));
                if (cartItems == null || !cartItems.Any())
                    return 0;

                decimal grandTotal = 0;
                foreach (var item in cartItems)
                {
                    grandTotal += item.quantity * item.amount;
                }

                return grandTotal;
            }
            catch
            {
                return 0;
            }
        }

        [HttpPost]
        public IActionResult ApplyCoupon(string couponCode)
        {
            try
            {
                var grandTotal = GetGrandTotal();
                var result = _repository.ApplyCoupon(couponCode, grandTotal);

                if (result.Success)
                {
                    HttpContext.Session.SetString("AppliedCoupon", couponCode);
                    HttpContext.Session.SetString("DiscountAmount", result.DiscountAmount.ToString());

                    TempData["Discount"] = result.DiscountAmount;
                    TempData["CouponCode"] = couponCode;
                    TempData["successmessage"] = result.Message;

                    // Update the cart with coupon details
                    UpdateCartWithCoupon(couponCode, result.DiscountAmount);
                }

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new CouponApplicationResult
                {
                    Success = false,
                    Message = "An error occurred while applying coupon"
                });
            }
        }

        public IActionResult UpdateCartWithCoupon(string couponCode, decimal discountAmount)
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (!string.IsNullOrEmpty(customerId))
            {
                using (var connection = new SqlConnection())
                {
                    _repository.UpdateCartWithCoupon(int.Parse(customerId), couponCode, discountAmount);
                    return Json(new { success = true });
                }
            }
            return Json(new { success = false, message = "Customer ID not found in session." });
        }

        [HttpPost]
        public JsonResult RemoveCoupon()
        {
            try
            {
                var couponCode = HttpContext.Session.GetString("AppliedCoupon");

                if (!string.IsNullOrEmpty(couponCode))
                {
                    // Remove coupon from cart
                    RemoveCouponFromCart();
                }

                HttpContext.Session.Remove("AppliedCoupon");
                HttpContext.Session.Remove("DiscountAmount");

                TempData.Remove("Discount");
                TempData.Remove("CouponCode");

                return Json(new { success = true, message = "Coupon removed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error removing coupon" });
            }
        }

        [HttpPost]
        public IActionResult RemoveCouponFromCart()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");

            if (!string.IsNullOrEmpty(customerId) && int.TryParse(customerId, out int customerIdInt))
            {
                _repository.RemoveCouponFromCart(customerIdInt);
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Customer ID not found in session." });
        }

        [HttpGet]
        public IActionResult GetCurrentCouponStatus()
        {
            try
            {
                var couponCode = HttpContext.Session.GetString("AppliedCoupon");
                var discountAmount = HttpContext.Session.GetString("DiscountAmount");

                if (!string.IsNullOrEmpty(couponCode) && !string.IsNullOrEmpty(discountAmount))
                {
                    return Json(new
                    {
                        HasCoupon = true,
                        Code = couponCode,
                        Discount = decimal.Parse(discountAmount)
                    });
                }

                return Json(new { HasCoupon = false });
            }
            catch
            {
                return Json(new { HasCoupon = false });
            }
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

        //[HttpPost]
        //public IActionResult InitiateOrder([FromBody] PaymentInitiateModel model)
        //{
        //    int customerId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

        //    try
        //    {
        //        var client = new RazorpayClient(_razorpaykey, _razorpaysecret);
        //        var options = new Dictionary<string, object>
        //        {
        //            { "amount", model.amount * 100 }, // Amount in paise
        //            { "currency", "INR" }
        //        };
        //        var order = client.Order.Create(options);
        //        string razorpayOrderId = order["id"].ToString();

        //        var orderItems = new List<tbl_order_items>();
        //        foreach (var item in model.OrderItems)
        //        {
        //            orderItems.Add(new tbl_order_items
        //            {
        //                menu_id = item.menu_id,
        //                quantity = item.quantity,
        //                list_price = item.listprice,
        //                discount = item.discount
        //            });
        //        }

        //        var createdOrder = _repository.CreateOrder(customerId, model.amount, razorpayOrderId, orderItems, model.address_id);

        //        return Json(new { orderId = razorpayOrderId });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { error = ex.Message });
        //    }
        //}

        [HttpPost]
        public IActionResult InitiateOrder([FromBody] PaymentInitiateModel model)
        {
            // Validate minimum amount (₹1)
            if (model.amount < 1)
            {
                return Json(new
                {
                    success = false,
                    message = "Order amount must be at least ₹1. Please add more items to your cart."
                });
            }

            try
            {
                int customerId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

                // 1. Initialize Razorpay client
                var client = new RazorpayClient(_razorpaykey, _razorpaysecret);

                // 2. Prepare order options
                int amountInPaise = (int)(model.amount * 100);
                string receiptId = $"ORD{DateTime.Now:yyyyMMddHHmmss}";

                var options = new Dictionary<string, object>
        {
            { "amount", amountInPaise },
            { "currency", "INR" },
            { "receipt", receiptId },
            { "payment_capture", 1 }
        };

                // 3. Create Razorpay order with proper error handling
                Razorpay.Api.Order razorpayOrder;
                try
                {
                    razorpayOrder = client.Order.Create(options);

                    // Validate the response
                    if (razorpayOrder == null || razorpayOrder.Attributes == null)
                    {
                        throw new Exception("Invalid response from Razorpay");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Razorpay order creation failed");
                    return Json(new
                    {
                        success = false,
                        message = "Payment gateway error. Please try again."
                    });
                }

                // 4. Safely get the order ID
                string razorpayOrderId;
                try
                {
                    razorpayOrderId = razorpayOrder.Attributes["id"].ToString();
                    if (string.IsNullOrEmpty(razorpayOrderId))
                    {
                        throw new Exception("Empty Razorpay order ID received");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get Razorpay order ID");
                    return Json(new
                    {
                        success = false,
                        message = "Failed to process payment. Please try again."
                    });
                }

                // 5. Prepare order items
                var orderItems = new List<tbl_order_items>();
                try
                {
                    foreach (var item in model.OrderItems)
                    {
                        orderItems.Add(new tbl_order_items
                        {
                            menu_id = item.menu_id,
                            quantity = item.quantity,
                            list_price = item.listprice,
                            discount = item.discount,
                            estimated_time = DateTime.Now.AddHours(1) // More realistic estimate
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Invalid order items");
                    return Json(new
                    {
                        success = false,
                        message = "Invalid items in cart. Please refresh and try again."
                    });
                }

                // 6. Create order in database
                try
                {
                    var createdOrder = _repository.CreateOrder(
                        customerId,
                        model.amount,
                        razorpayOrderId,
                        orderItems,
                        model.res_id,
                        model.address_id
                    );

                    return Json(new
                    {
                        success = true,
                        orderId = razorpayOrderId,
                        amount = amountInPaise,
                        receipt = receiptId
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,"Database order creation failed");
                    return Json(new
                    {
                        success = false,
                        message = "Failed to save order. Please contact support."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order initiation failed");
                return Json(new
                {
                    success = false,
                    message = "An unexpected error occurred. Please try again later."
                });
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
                    _repository.SavePayment(model.razorpay_order_id, model.razorpay_payment_id, model.amount, "Pending");
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

        [HttpPost]
        public IActionResult UpdateCartItemQuantity(int cartItemId, int quantity)
        {
            if (cartItemId <= 0 || quantity <= 0)
            {
                return BadRequest("Invalid input");
            }

            bool updated = _repository.UpdateCartItemQuantity(cartItemId, quantity);

            if (updated)
            {
                return Ok(new { success = true, message = "Quantity updated" });
            }
            else
            {
                return NotFound(new { success = false, message = "Cart item not found" });
            }
        }

        public IActionResult ThankYou()
        {
            return View();
        }

        public IActionResult CustomerOrderhistory()
        {
            int customerId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            var orders = _repository.GetUserOrdersWithItemsAndImages(customerId);
            return View(orders);
        }

        [HttpPost]
        public IActionResult SubmitRating([FromBody] tbl_ratings rating)
        {
            // Get customer ID from session
            rating.CustomerId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            bool success = _repository.SubmitReview(rating);
            return Json(new { success = success });
        }

        public IActionResult GenerateBill(int orderId)
        {
            var pdfBytes = _repository.GenerateBill(orderId);
            return File(pdfBytes, "application/pdf", $"Bill_{orderId}.pdf");
        }


        //-----------------------------Track Order Item----------------------
        //public IActionResult TrackOrder(int id)
        //{
        //    int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

        //    if(userId == 0)
        //    {
        //        return RedirectToAction("Locality", "Locality");
        //    }

        //    var order = _repository.GetOrderTrackingDetails(id, userId);

        //    if(order == null)
        //    {
        //        return NotFound("Order not found");
        //    }

        //    var statusHistory = _repository.GetOrderStatusHistory(id);
        //    var deliveryPartnerInfo = _repository.GetDeliveryPartnerInfo(id);

        //    var viewModel = new OrderTrackingViewModel
        //    {
        //        order = order,
        //        orderStatusHistory = statusHistory,
        //        deliveryPartner = deliveryPartnerInfo
        //    };

        //    return View(viewModel);
        //}
        public ActionResult GetOrderTrackingDetails(int orderId)
        {
            // Get the current user ID from session
            int userId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            // Get order details
            var order = _repository.GetOrderTrackingDetails(orderId, userId);
            if (order == null) return Json(new { success = false, message = "Order not found" });

            // Get status history
            var statusHistory = _repository.GetOrderStatusHistory(orderId);

            // Get delivery partner info if order is on the way
            DeliveryPartnerInfo deliveryPartner = null;
            if (order.AssignmentStatus == "OnTheWay" || order.AssignmentStatus == "Accepted")
            {
                deliveryPartner = _repository.GetDeliveryPartnerInfo(orderId);
            }

            return Json(new
            {
                success = true,
                order = new
                {
                    Id = order.order_id,
                    Status = order.order_status,
                    CreatedDate = order.order_date.ToString("dd MMM yyyy hh:mm tt"),
                    GrandTotal = order.grand_total,
                    CustomerName = order.customer_name,
                    DeliveryAddress = order.delivery_address,
                    RestaurantName = order.restaurant_name,
                    RestaurantAddress = order.restaurant_street
                },
                statusHistory = statusHistory.Select(sh => new {
                    Status = sh.order_status,
                    StatusDate = sh.order_date.ToString("dd MMM yyyy hh:mm tt")
                }),
                deliveryPartner = deliveryPartner != null ? new
                {
                    PartnerName = deliveryPartner.delivery_partner_name,
                    PhoneNumber = deliveryPartner.delivery_partner_phone,
                    VehicleNumber = deliveryPartner.vehicle_number,
                    PhotoUrl = deliveryPartner.PhotoUrl
                } : null
            });
        }

        [HttpGet]
        public IActionResult GetOrderStatusUpdates(int orderId)
        {
            var statusHistory = _repository.GetOrderStatusHistory(orderId);
            return Json(statusHistory);
        }
    }
}
