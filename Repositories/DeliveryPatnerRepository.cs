using ClosedXML.Excel;
using Foodie.Models.DeliveryPartner;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Foodie.Repositories
{
    public class DeliveryPatnerRepository : IDeliveryPatnerRepository
    {
        private readonly string _connectionString;

        public DeliveryPatnerRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Defaultconnection");
        }

        public bool AcceptOrder(int partnerId, int restaurantId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("customers.sp_GetAssignedOrders", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@partner_id", partnerId);
                    command.Parameters.AddWithValue("@restaurant_id", restaurantId);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool CancelOrder(int orderId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.OpenAsync();
                using (var command = new SqlCommand("sp_CancelAssignedOrder", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@OrderId", orderId);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public MemoryStream ExportOrdersToExcel(int partnerId)
        {
            var orders = GetAssignedOrdersAsync(partnerId);
            var stream = new MemoryStream();

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

                workbook.SaveAs(stream);
                stream.Position = 0;
            }

            return stream;
        }

        public MemoryStream ExportOrdersToPdf(int partnerId)
        {
            var orders = GetAssignedOrdersAsync(partnerId);
            var stream = new MemoryStream();

            var pdfDoc = new Document(PageSize.A4, 10f, 10f, 20f, 20f);
            PdfWriter.GetInstance(pdfDoc, stream).CloseStream = false;

            pdfDoc.Open();
            pdfDoc.Add(new Paragraph("Assigned Orders Report"));
            pdfDoc.Add(new Paragraph(" "));

            var table = new PdfPTable(7);
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

            stream.Position = 0;
            return stream;
        }

        public List<AssignedOrderViewModel> GetAssignedOrdersAsync(int partnerId)
        {
            var orders = new List<AssignedOrderViewModel>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("customers.sp_GetAssignedOrders", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PartnerId", partnerId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var order = new AssignedOrderViewModel
                            {
                                OrderId = reader.GetInt32(reader.GetOrdinal("order_id")),
                                OrderStatus = reader.GetString(reader.GetOrdinal("order_status")),
                                FoodStatus = reader.GetString(reader.GetOrdinal("food_status")),
                                OrderDate = reader.GetDateTime(reader.GetOrdinal("order_date")),
                                RestaurantName = reader.GetString(reader.GetOrdinal("restaurant_name")),
                                RestaurantLat = reader.GetString(reader.GetOrdinal("restaurant_lat")),
                                RestaurantLag = reader.GetString(reader.GetOrdinal("restaurant_lag")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("customer_id")),
                                CustomerLatitude = reader.GetString(reader.GetOrdinal("latitude")),
                                CustomerLongitude = reader.GetString(reader.GetOrdinal("longitude")),
                                AddressType = reader.GetString(reader.GetOrdinal("addresses_type")),
                                CityId = reader.GetInt32(reader.GetOrdinal("CityId")),
                                StateId = reader.GetInt32(reader.GetOrdinal("StateId")),
                                DistrictId = reader.GetInt32(reader.GetOrdinal("DistrictId")),
                                DistanceInKm = reader.GetDouble(reader.GetOrdinal("distance_km"))
                            };
                            orders.Add(order);
                        }
                    }
                }
                connection.Close();
            }
            return orders;
        }
        public List<tbl_deliveryNotification> GetNotifications(int partnerId)
        {
            var notificatons = new List<tbl_deliveryNotification>();

            using(SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT TOP 5 n.notification_id, n.MessageType, n.MessageContent, n.CreatedAT, n.NotificationType
                                FROM deliverypartner.tbl_deliveryNotification n
                                INNER JOIN deliverypartner.tbl_deliveryRequest r ON r.order_id = n.AssignmentID
                                WHERE r.partner_id = @PartnerId AND n.IsRead = 0
                                ORDER BY n.CreatedAT DESC   ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PartnerId", partnerId);

                using(SqlDataReader rd = cmd.ExecuteReader())
                {
                    conn.Open();
                    while (rd.Read())
                    {
                        notificatons.Add(new tbl_deliveryNotification
                        {
                            notification_id = rd.GetInt32(rd.GetOrdinal("notification_id")),
                            MessageType = rd["MessageType"].ToString(),
                            MessageContent = rd["MessageContent"].ToString(),
                            CreatedAT = Convert.ToDateTime(rd["createdAT"]),
                            IsRead = Convert.ToBoolean(rd["IsRead"]),
                            NotificationType = rd["NotifictionType"].ToString()
                        });
                    }
                }
            }
            return notificatons;
        }

        public double GetAverageDeliveryTimeAsync(int partnerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                    string query = @"SELECT ISNULL(AVG(DATEDIFF(MINUTE, o.deliver_dateTime, ad.AssignedAt)), 0)
                  FROM customers.tbl_orders o
                  JOIN admins.tbl_deliveryassignments ad on o.order_id = ad.order_id
                  WHERE ad.partner_id = @PartnerId
                  AND o.deliver_dateTime > DATEADD(DAY, -30, GETDATE())";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@PartnerId", partnerId);

                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    return Convert.ToDouble(result);
            }
        }

        public double GetAverageRatingAsync(int partnerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT ISNULL(AVG(CustomerRating), 0)
                      FROM deliverypartner.tbl_deliveryRatings dr
                      JOIN admins.tbl_deliveryassignments ad on ad.partner_id = dr.partner_id
                      JOIN customers.tbl_orders o on o.order_id = ad.order_id
                      WHERE o.order_id IN (
                          SELECT order_id FROM customers.tbl_orders WHERE dr.partner_id = @PartnerId
                      )";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PartnerId", partnerId);

                conn.Open();
                var result = cmd.ExecuteScalar();
                return Convert.ToDouble(result);
            }
        }

        public LatestOrderInfo GetLatestOrderAsync(int partnerId)
        {
            using(SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT TOP 1 o.order_date, r.restaurant_name AS RestaurantName, c.customer_name AS CustomerName, 
                      ca.area+' '+ca.landmark as [Address], COUNT(oi.order_items_id) AS ItemCount, COUNT(o.order_id) AS OrderCount
                      FROM customers.tbl_orders o
                      JOIN customers.tbl_order_items oi ON o.order_id = oi.order_id
                      JOIN vendores.tbl_restaurant r ON o.resturant_id = r.restaurant_id
                      JOIN customers.tbl_customer c ON o.customer_id = c.customer_id
                      JOIN admins.tbl_deliveryassignments ad ON ad.order_id = o.order_id
                      JOIN customers.tbl_address ca ON ca.customer_id = c.customer_id
                      WHERE ad.partner_id = @PartnerId
                      GROUP BY o.order_date, r.restaurant_name, c.customer_name, 
                      ca.area+' '+ca.landmark
                      ORDER BY o.order_date DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PartnerId", partnerId);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new LatestOrderInfo
                        {
                            OrderDate = reader.GetDateTime(0),
                            RestaurantName = reader.GetString(1),
                            CustomerName = reader.GetString(2),
                            Address = reader.GetString(3),
                            ItemCount = reader.GetInt32(4)
                        };
                    }
                }
                return null;
            }
        }


        public int GetTodayDeliveriesAsync(int partnerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT COUNT(o.order_id)
                  FROM customers.tbl_orders o
                  JOIN admins.tbl_deliveryassignments da on o.order_id = da.order_id
                  WHERE da.partner_id = @PartnerId 
                  AND CAST(o.order_date AS DATE) = CAST(GETDATE() AS DATE)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PartnerId", partnerId);

                conn.Open();
                var result = cmd.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }

        public decimal GetTodayEarningsAsync(int partnerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT ISNULL(SUM(o.grand_total), 0)
                  FROM customers.tbl_orders o
                  JOIN admins.tbl_deliveryassignments da on o.order_id = da.order_id
                  WHERE da.partner_id = @PartnerId 
                  AND CAST(o.order_date AS DATE) = CAST(GETDATE() AS DATE)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PartnerId", partnerId);

                conn.Open();
                var result = cmd.ExecuteScalar();
                return Convert.ToDecimal(result);
            }
        }

        public bool MarkOrderAsDelivered(int orderId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("sp_MarkOrderAsDelivered", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateOnlineStatus(int partnerId, bool isOnline)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE deliverypartner.tbl_partners
                    SET is_online = @IsOnline,
                        last_online_update = GETDATE()
                    WHERE partner_id = @PartnerId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PartnerId", partnerId);
                cmd.Parameters.AddWithValue("@IsOnline", isOnline);

                conn.Open();
                int affectedRows = cmd.ExecuteNonQuery();
                return affectedRows > 0;
            }
        }

        public bool UpdateOrderStatus(int orderId, string status)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Update order status
                string updateOrderQuery = @"UPDATE customers.tbl_orders 
                    SET order_status = @Status, 
                        deliver_dateTime = CASE WHEN @Status = 'Delivered' THEN GETDATE() ELSE deliver_dateTime END
                    WHERE order_id = @OrderId";

                using (SqlCommand cmd = new SqlCommand(updateOrderQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    cmd.ExecuteNonQuery();
                }

                // Update delivery assignment
                string updateAssignmentQuery = @"UPDATE admins.tbl_deliveryassignments
                    SET status = @Status,
                        updated_at = GETDATE()
                    WHERE order_id = @OrderId";

                using (SqlCommand cmd = new SqlCommand(updateAssignmentQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
        }
    }
}
