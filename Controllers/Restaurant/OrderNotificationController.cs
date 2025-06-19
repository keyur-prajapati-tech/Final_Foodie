using Foodie.Models;
using Foodie.Models.customers;
using Foodie.Models.Restaurant;
using Foodie.Repositories;
using Foodie.ViewModels;
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

        [HttpGet("GetMenuHighlights")]
        public JsonResult GetMenuHighlights()
        {
            var menuItems = _repository.GetHighlightMenuItem();
            return Json(menuItems);
        }

        [HttpGet("GettopSellingItems")]
        public JsonResult GettopSellingItems()
        {
            var MenuItems = _repository.GetTopSellingMenuItems();
            return Json(MenuItems);
        }

        [HttpGet("GetRecentOrders")]
        public JsonResult GetRecentOrders()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            var orders = _repository.GetrecentOrders(restaurantId);
            return Json(new { success = true, data = orders });
        }

        [HttpGet("GetPopularItems")]
        public JsonResult GetPopularItems()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            var items = _repository.GetPopularMenuItems(restaurantId);
            return Json(new { success = true, data = items });
        }

        [HttpGet("GetDashboardData")]
        public IActionResult GetDashboardData()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            var viewModel = new RestaurantDashboardViewModel
            {
                Stats = _repository.GetDashboardStats(restaurantId),
                RecentOrders = _repository.GetrecentOrders(restaurantId),
                CuisineStats = _repository.GetCuisineStats(restaurantId),
                PopularItems = _repository.GetPopularMenuItems(restaurantId)
                        .Select(item => new PopularItem_ViewModel
                        {
                            
                            MenuId = item.MenuId,
                            MenuName = item.MenuName,
                            ImageUrl = item.MenuImageBase64,
                            Category = item.cuisine_name,
                            OrderCount = item.TotalQuantitySold,
                            Price = item.Amount
                        }).ToList(),
                Cuisines = _repository.GetCuisines(restaurantId)
            };

            return Json(viewModel);
        }

        [HttpGet("GetMenuWiseStats")]
        public IActionResult GetMenuWiseStats(string timePeriod = "week")
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));

            var stats = _repository.GetMenuWiseOrderstats(restaurantId, timePeriod);

            // Initialize model if null
            if (stats == null)
            {
                stats = new MenuStatsResponse
                {
                    CategoryStats = new List<MenuCategoryStatsViewModel>(),
                    Totals = new MenuCategoryStatsViewModel
                    {
                        CuisineName = "Total",
                        OrderCount = 0,
                        Revenue = 0,
                        AvgOrderValue = 0,
                        PercentageOfTotal = 0
                    },
                    TimePeriod = timePeriod,
                    StartDate = DateTime.Now.AddDays(-7),
                    EndDate = DateTime.Now
                };
            }

            return PartialView("_MenuWiseStatsPartial", stats);
        }

        [HttpPost]
        public async Task<IActionResult> GetMenuWiseStatsJson(string timePeriod)
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            var stats = _repository.GetMenuWiseOrderstats(restaurantId, timePeriod);
            return Ok(stats);
        }

        private int? GetCurrentRestaurantId()
        {
            var restaurantIdString = HttpContext.Session.GetString("RestaurantId");
            if (string.IsNullOrEmpty(restaurantIdString) || !int.TryParse(restaurantIdString, out int restaurantId))
            {
                return null;
            }
            return restaurantId;
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
                if (restaurantId <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid restaurant ID"
                    });
                }

                int onlineStatus = _repository.getOnline(restaurantId);

                return Json(new
                {
                    success = true,
                    isOnline = onlineStatus == 1, // Convert to boolean if that's your expected output
                    statusCode = onlineStatus    // Optional: include the raw status if needed
                });
            }
            catch (Exception ex)
            {
                // Consider logging the error here
                return Json(new
                {
                    success = false,
                    message = "Unable to check online status" // More user-friendly message
                });
            }
        }

        [HttpGet]
        [Route("isApproved")]
        public JsonResult isApproved(int restaurantId)
        {
            try
            {
                // Simple validation
                if (restaurantId < 1)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Restaurant ID must be greater than 0"
                    });
                }

                bool data = _repository.isApprove(restaurantId);

                return Json(new
                {
                    success = true,
                    isApproved = data,
                    message = data ? "Restaurant is approved" : "Restaurant is not approved"
                });
            }
            catch (SqlException sqlEx)
            {
                return Json(new
                {
                    success = false,
                    message = "Database error occurred"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while processing your request"
                });
            }
        }

        [HttpPost]
        [Route("UpdateOnlineStatus")]
        public IActionResult UpdateOnlineStatus([FromBody] tbl_restaurant request)
        {
            if (request == null || request.restaurant_id <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid data" });
            }

            bool updated = _repository.UpdateOnlineStatus(request.restaurant_id, request.restaurant_isOnline);

            return Ok(new
            {
                success = updated,
                message = updated ? "Status updated" : "Update failed"
            });
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