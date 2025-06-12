using Foodie.Models;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Foodie.Controllers.Restaurant
{
    [Route("Restaurant")]
    public class OrderNotificationController : Controller
    {
        private readonly IRestaurantRepository _repository;

        public OrderNotificationController(IRestaurantRepository repository)
        {
            _repository = repository;
        }

        [Route("Dashboard")]
        public IActionResult Dashboard()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            ViewBag.RestaurantId = restaurantId;
            return View();
        }

        [Route("OrderNoti")]
        public IActionResult OrderNoti()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            ViewBag.RestaurantId = restaurantId;    
            return View();
        }

        [Route("OrderReady")]
        public IActionResult OrderReady()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId")); ;
            var orders = _repository.tbl_Orders_Notifis_Accepted(restaurantId);

            return View(orders);
        }

        [Route("OrderPickedUp")]
        public IActionResult OrderPickedup()
        {
            return View();
        }

        [HttpGet]
        [Route("CheckNewOrders")]
        public JsonResult CheckNewOrders(int restaurantId)
        {
            try
            {
                var orders = _repository.tbl_Orders_Notifis(restaurantId);

                if (orders == null || orders.Count == 0)
                {
                    return Json(new { success = false, message = "No new orders found." });
                }

                // Group by order_id and get menu names
                var formattedOrders = orders
                    .GroupBy(o => o.order_id)
                    .Select(g => new {
                        orderId = g.Key,
                        customerName = g.First().customer_name,
                        orderTime = g.First().estimated_time.ToString("hh:mm tt"),
                        items = g.Select(i => new {
                            name = _repository.GetMenuItemName(i.menu_id), // USING THE METHOD HERE
                            quantity = i.quantity,
                            price = i.list_price
                        }),
                        total = g.Sum(i => i.list_price * i.quantity)
                    }).ToList();

                return Json(new { success = true, orders = formattedOrders });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }

        }

        [HttpPost]
        [Route("acceptOrder")]
        public IActionResult acceptOrder(int order_id, string food_status)
        {
            try
            {
                var result = _repository.AcceptOrder(order_id, food_status);
                if (result > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = food_status == "ACCEPT"
                            ? "Order accepted successfully."
                            : "Order rejected successfully."
                    });
                }
                return Json(new
                {
                    success = false,
                    message = "Failed to update order status."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        [HttpPost]
        [Route("isOnline")]
        public JsonResult isOnline(int status)
        {
            try
            {
                var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
                var result = _repository.IsOnline(restaurantId, status);

                return Json(new
                {
                    success = result > 0,
                    message = status == 1
                        ? "Restaurant is now offline."
                        : "Restaurant is now online."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        [HttpGet]
        [Route("getOnline")]
        public JsonResult getOnline(int restaurantId)
        {
            try
            {
                var data = _repository.getOnline(restaurantId);
                return Json(new
                {
                    success = true,
                    isOnline = data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        [HttpGet]
        [Route("isApproved")]
        public JsonResult isApproved(int restaurantId)
        {
            try
            {
                var data = _repository.isApprove(restaurantId);
                return Json(new
                {
                    success = true,
                    isApproved = data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        [HttpPost]
        [Route("OrderReady")]
        public JsonResult OrderReady(int order_id, int restaurant_id)
        {
            try
            {
                var result = _repository.OrderReady(order_id, restaurant_id);
                if (result > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Order is ready."
                    });
                }
                return Json(new
                {
                    success = false,
                    message = "Failed to update order status."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }
        //[HttpGet]
        //[Route("getOrderReady")]
        //public JsonResult getOrderReady(int restaurantId)
        //{
        //    var orders = _repository.tbl_Orders_Notifis_Ready(restaurantId);
        //    if (orders == null || orders.Count == 0)
        //    {
        //        return Json(new { success = false, message = "No new orders found." });
        //    }
        //    return Json(orders);
        //}

        //[HttpGet]
        //[Route("getOrderPickedUp")]
        //public JsonResult getOrderPickedUp(int restaurantId)
        //{
        //    var orders = _repository.tbl_Orders_Notifis_Picked(restaurantId);
        //    if (orders == null || orders.Count == 0)
        //    {
        //        return Json(new { success = false, message = "No new orders found." });
        //    }
        //    return Json(orders);
        //}



        [HttpPost]
        public IActionResult InsertFeedback(decimal rating, string feedbackDescription)
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            if (restaurantId == null)
                return BadRequest("Restaurant ID not found in session.");

            try
            {
                var feedback = new tbl_vendor_feedback
                {
                    restaurant_id = restaurantId,
                    rating = rating,
                    feedback_description = feedbackDescription
                };
                _repository.InsertFeedback(feedback);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}