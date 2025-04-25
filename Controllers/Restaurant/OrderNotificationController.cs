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

        [Route("OrderNoti")]
        public IActionResult OrderNoti()
        {
            return View();
        }

        [Route("OrderReady")]
        public IActionResult OrderReady()
        {
            var orders = _repository.tbl_Orders_Notifis_Accepted(1);

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
            var orders = _repository.tbl_Orders_Notifis(restaurantId);
            if (orders == null || orders.Count == 0)
            {
                return Json(new { success = false, message = "No new orders found." });
            }

            return Json(orders);

        }

        [HttpPost]
        [Route("acceptOrder")]
        public IActionResult acceptOrder(int order_id,string food_status)
        {
            
            var result = _repository.AcceptOrder(order_id,food_status);
            if (result > 0)
            {
                return Json(new { success = true, message = "Order accepted successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to accept order." });
            }

        }

       
        [HttpPost]
        [Route("isOnline")]
        public JsonResult isOnline(int status)
        {
            var restaurantId = 1;

            if (status == 1)
            {
                var result = _repository.IsOnline(restaurantId, 1);

                return Json(new { success = true, message = "Restaurant is offline." });

            }
            else
            {
                var result = _repository.IsOnline(restaurantId, 0);

                return Json(new { success = true, message = "Restaurant is online." });

            }

        }

        [HttpGet]
        [Route("getOnline")]
        public JsonResult getOnline(int restaurantId)
        {
            var data = _repository.getOnline(restaurantId);

            return Json(data);

        }
    }
}