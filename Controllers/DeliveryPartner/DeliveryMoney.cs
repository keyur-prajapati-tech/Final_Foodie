
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Configuration;
using System.Reflection.Metadata;
using Foodie.Models.DeliveryPartner;
using Foodie.Repositories;

[Route("deliverymoney")]
public class DeliveryMoney : Controller
{
    private readonly string _connStr;
    private readonly IDeliveryPatnerRepository _deliveryrepository;

    public DeliveryMoney(IConfiguration configuration, IDeliveryPatnerRepository deliveryrepository)
    {
        _connStr = configuration.GetConnectionString("DefaultConnection");
        _deliveryrepository = deliveryrepository;
    }

    [Route("m/wallet")]
    public IActionResult Wallet() => View();

    [Route("m/earn")]
    public IActionResult Earnings()
    {
        return View();
    }

    [HttpGet]
    [Route("GetWeeklyEarnings")]
    public IActionResult GetWeeklyEarnings()
    {

        int PartnerId = Convert.ToInt16(HttpContext?.Session.GetInt32("PartnerId"));
        var earnings = GetTopEarningsByPartner(PartnerId);
        return Ok(earnings);
    }
    public List<tbl_deliveryEarnings> GetTopEarningsByPartner(int partnerId)
    {
        List<tbl_deliveryEarnings> list = new List<tbl_deliveryEarnings>();

        using (SqlConnection conn = new SqlConnection(_connStr))
        {
            SqlCommand cmd = new SqlCommand("[deliverypartner].[sp_GetTop100EarningsByPartner]", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            // Add parameter
            cmd.Parameters.AddWithValue("@PartnerId", partnerId);

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new tbl_deliveryEarnings
                {
                    earning_id = Convert.ToInt32(reader["earning_id"]),
                    partner_id = Convert.ToInt32(reader["partner_id"]),
                    request_id = Convert.ToInt32(reader["request_id"]),
                    Earnings = Convert.ToDecimal(reader["Earnings"]),
                    PaymentDate = Convert.ToDateTime(reader["PaymentDate"]),
                    PaymentStatus = reader["PaymentStatus"].ToString(),
                    mode_id = Convert.ToInt32(reader["mode_id"]),
                    TotalOrders = Convert.ToInt32(reader["TotalOrders"]) // new field
                });
            }
        }

        return list;
    }
    [HttpPost]
    public JsonResult GetEarningsData(string filter)
    {
        var earningsData = new List<SupportViewModel>();

        using (SqlConnection conn = new SqlConnection(_connStr))
        using (SqlCommand cmd = new SqlCommand("deliverypartner.GetEarningsSummary", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@PartnerId", 1); // Example partner_id
            cmd.Parameters.AddWithValue("@Filter", filter);

            conn.Open();
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    earningsData.Add(new SupportViewModel
                    {
                        PeriodLabel = reader["PeriodLabel"].ToString(),
                        TotalOrders = Convert.ToInt32(reader["TotalOrders"]),
                        TotalEarnings = Convert.ToDecimal(reader["TotalEarnings"])
                    });
                }
            }
        }

        return Json(earningsData);
    }
     int? PartnerId => HttpContext?.Session.GetInt32("PartnerId");

    [HttpGet("m/order")]
    public IActionResult Assigned_Orders()
    {
        if (PartnerId == null) return Unauthorized();

        var orders = _deliveryrepository.GetAssignedOrdersAsync(PartnerId.Value);
        return View(orders);
    }

    private List<AssignedOrderViewModel> GetStaticAssignedOrders()
    {
        return new List<AssignedOrderViewModel>
        {
            new AssignedOrderViewModel
            {
                OrderId = 101,
                OrderStatus = "Pending",
                FoodStatus = "Preparing",
                OrderDate = DateTime.Now.AddMinutes(-15),
                RestaurantName = "Pizza Palace",
                RestaurantLat = "21.125313987662086",
                RestaurantLag = "73.10879020537718",
                CustomerId = 201,
                CustomerLatitude = "21.126115860318098",
                CustomerLongitude = "73.11893968428953",
                AddressType = "Hotel",
                CityId = 1,
                StateId = 1,
                DistrictId = 1,
                DistanceInKm = 2.5,
                EstimatedDeliveryTime = "10 mins"
            },
            new AssignedOrderViewModel
        {
            OrderId = 102,
            OrderStatus = "Assigned",
            FoodStatus = "Ready",
            OrderDate = DateTime.Now.AddMinutes(-30),
            RestaurantName = "Burger Byte",
            RestaurantLat = "21.143833512209984",
            RestaurantLag = "73.09164016093595",
            CustomerId = 202,
            CustomerLatitude = "21.13617377097601",
            CustomerLongitude = "73.09446452702863",
            AddressType = "Home",
            CityId = 1,
            StateId = 1,
            DistrictId = 1,
            DistanceInKm = 3.1,
            EstimatedDeliveryTime = "12 mins"
        }
        };
    }

    public double CalculateDistanceInKm(double lat1, double lon1, double lat2, double lon2)
    {
        double R = 6371; // km
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Math.Round(R * c, 2);
    }

    public double ToRadians(double deg) => deg * (Math.PI / 180);

    public string EstimateDeliveryTime(double distanceInKm)
    {
        double averageSpeed = 30.0; // in km/h
        double timeInMinutes = (distanceInKm / averageSpeed) * 60;
        return $"{Math.Ceiling(timeInMinutes)} mins";
    }

    [HttpGet("export-excel")]
    public ActionResult ExportToExcel()
    {
        var orders = GetStaticAssignedOrders();

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Assigned Orders");
            var currentRow = 1;

            worksheet.Cell(currentRow, 1).Value = "Order ID";
            worksheet.Cell(currentRow, 2).Value = "Order Status";
            worksheet.Cell(currentRow, 3).Value = "Food Status";
            worksheet.Cell(currentRow, 4).Value = "Order Date";
            worksheet.Cell(currentRow, 5).Value = "Restaurant";
            worksheet.Cell(currentRow, 6).Value = "Customer ID";
            worksheet.Cell(currentRow, 7).Value = "Distance (km)";
            worksheet.Cell(currentRow, 8).Value = "Est. Delivery Time";

            foreach (var order in orders)
            {
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = order.OrderId;
                worksheet.Cell(currentRow, 2).Value = order.OrderStatus;
                worksheet.Cell(currentRow, 3).Value = order.FoodStatus;
                worksheet.Cell(currentRow, 4).Value = order.OrderDate.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cell(currentRow, 5).Value = order.RestaurantName;
                worksheet.Cell(currentRow, 6).Value = order.CustomerId;
                worksheet.Cell(currentRow, 7).Value = order.DistanceInKm;
                worksheet.Cell(currentRow, 8).Value = order.EstimatedDeliveryTime;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AssignedOrders.xlsx");
            }
        }
    }

    [HttpGet("export-pdf")]
    public ActionResult ExportToPDF()
    {
        var orders = GetStaticAssignedOrders();
        using (MemoryStream stream = new MemoryStream())
        {
            iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(PageSize.A4, 10f, 10f, 20f, 20f);
            PdfWriter.GetInstance(pdfDoc, stream).CloseStream = false;

            pdfDoc.Open();
            pdfDoc.Add(new Paragraph("Assigned Orders Report"));
            pdfDoc.Add(new Paragraph(" "));

            PdfPTable table = new PdfPTable(7);
            table.WidthPercentage = 100;

            string[] headers = { "Order ID", "Order Status", "Food Status", "Order Date", "Restaurant", "Customer ID", "Distance (km)" };
            foreach (var header in headers)
                table.AddCell(new Phrase(header));

            foreach (var order in orders)
            {
                table.AddCell(order.OrderId.ToString());
                table.AddCell(order.OrderStatus);
                table.AddCell(order.FoodStatus);
                table.AddCell(order.OrderDate.ToString("yyyy-MM-dd HH:mm"));
                table.AddCell(order.RestaurantName);
                table.AddCell(order.CustomerId.ToString());
                table.AddCell(order.DistanceInKm.ToString("F2"));
            }

            pdfDoc.Add(table);
            pdfDoc.Close();
            return File(stream.ToArray(), "application/pdf", "AssignedOrders.pdf");
        }
    }

    [HttpPost("update-status")]
    public ActionResult UpdateOrderStatus(int OrderId, string status)
    {
        using (SqlConnection conn = new SqlConnection(_connStr))
        {
            using (SqlCommand cmd = new SqlCommand("deliverypartner.sp_UpdateOrderStatus", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@OrderId", OrderId);
                cmd.Parameters.AddWithValue("@NewStatus", status);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        TempData["Success"] = $"Order #{OrderId} status updated to {status}";
        return RedirectToAction("Assigned_Orders");
    }

    [HttpPost("delivered")]
    public IActionResult MarkAsDelivered(int orderId)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(_connStr))
            {
                SqlCommand cmd = new SqlCommand("sp_MarkOrderAsDelivered", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            TempData["Success"] = "Order marked as delivered.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Something went wrong!";
        }
        return RedirectToAction("Assigned_Orders");
    }

    [HttpPost("cancel-order")]
    public IActionResult CancelOrder(int orderId)
    {
        try
        {
            using (SqlConnection con = new SqlConnection(_connStr))
            {
                SqlCommand cmd = new SqlCommand("sp_CancelAssignedOrder", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            TempData["Success"] = "Order cancelled.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Something went wrong!";
        }
        return RedirectToAction("Assigned_Orders");
    }

    [HttpPost("accept-order")]
    public IActionResult AcceptOrder([FromBody] int restaurantId)
    {
        int? partnerId = HttpContext.Session.GetInt32("PartnerId");
        if (partnerId == null)
            return Unauthorized();

        using (SqlConnection conn = new SqlConnection(_connStr))
        {
            conn.Open();
            SqlCommand cmd = new SqlCommand("deliverypartner.sp_AssignOrderToPartner", conn); // create this SP
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@partner_id", partnerId);
            cmd.Parameters.AddWithValue("@restaurant_id", restaurantId);
            cmd.ExecuteNonQuery();
        }

        return Ok();
    }

    [HttpGet("d/notifications")]
    public IActionResult GetNotifications()
    {
        var partnerId = HttpContext.Session.GetInt32("PartnerId");
        if (partnerId == null)
            return Unauthorized();

        List<dynamic> notifications = new();

        using (SqlConnection con = new SqlConnection(_connStr))
        {
            using (SqlCommand cmd = new SqlCommand(@"
            SELECT TOP 5 n.notification_id, n.MessageType, n.MessageContent, n.CreatedAT, n.NotificationType
            FROM deliverypartner.tbl_deliveryNotification n
            INNER JOIN deliverypartner.tbl_deliveryRequest r ON r.request_id = n.AssignmentID
            WHERE r.partner_id = @partnerId AND n.IsRead = 0
            ORDER BY n.CreatedAT DESC", con))
            {
                cmd.Parameters.AddWithValue("@partnerId", partnerId);
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        notifications.Add(new
                        {
                            Id = Convert.ToInt32(reader["notification_id"]),
                            Type = reader["MessageType"].ToString(),
                            Content = reader["MessageContent"].ToString(),
                            Created = Convert.ToDateTime(reader["CreatedAT"]).ToString("g"),
                            Style = reader["NotificationType"].ToString()
                        });
                    }
                }
            }
        }

        return Json(notifications);
    }

}

