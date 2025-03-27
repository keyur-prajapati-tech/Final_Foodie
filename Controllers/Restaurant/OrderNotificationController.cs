using Foodie.Models;
using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Restaurant
{
    [Route("Restaurant")]
    public class OrderNotificationController : Controller
    {
        [Route("OrderNoti")]
        public IActionResult OrderNoti()
        {
            return View();
        }

        [Route("OrderReady")]
        public IActionResult OrderReady()
        {
            return View();
        }

        [Route("OrderPickedUp")]
        public IActionResult OrderPickedup()
        {
            return View();
        }

        //[HttpPost]
        //public JsonResult ReceiveOrder(OrderModel order)
        //{
        //    // Save the order to the database
        //    // Simulate order processing
        //    Console.WriteLine("Received order for restaurant");

        //    return Json(new { success = true, message = "Order received successfully" });
        //}
        [HttpPost]
        public JsonResult ReceiveOrder(OrderModel order)
        {
            if (order == null || order.Items.Count == 0)
            {
                return Json(new { success = false, message = "Invalid order details" });
            }

            // Save the order to the database (you can implement this logic)

            return Json(new { success = true });
        }

        [Route("popup")]
        public ActionResult displaypopup()
        {
            // Fetch orders from the database (dummy data for now)
            List<OrderItem> orders = new List<OrderItem>
    {
        new OrderItem { Name = "Dal Kachori", Quantity = 1, Price = 120 },
        new OrderItem { Name = "Naylon Khaman", Quantity = 1, Price = 120 }
    };

            ViewBag.Orders = orders; // Pass orders to the view
            return View();
        }
    }
}