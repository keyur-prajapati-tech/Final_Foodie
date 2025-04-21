using Foodie.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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

        public JsonResult CheckNewOrders(int restaurantId)
        {
            List<object> orders = new List<object>();

            string connectionString = "Data Source=SSMOZ9;Initial Catalog=order_demo;User ID=sa;Password=SSM@123;Encrypt=False";
            //"Data Source=RISHI\\SSMSERVER;Initial Catalog=order_demo;User ID=sa;Password=rishi8102;Encrypt=False";

            string query = "SELECT OrderId, OrderDetails FROM Orders WHERE RestaurantId = @RestaurantId AND Status = 'Pending'";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@RestaurantId", restaurantId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    orders.Add(new
                    {
                        OrderId = reader["OrderId"],
                        OrderDetails = reader["OrderDetails"].ToString()
                    });
                }

                conn.Close();
            }

            return Json(orders);
        }

    }
}