// Controllers/Admin/DeliveryPartnerManagementController.cs
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Data;
using Microsoft.Extensions.Configuration;
using Foodie.Models.DeliveryPartner;

namespace Foodie.Controllers.Admin
{
    //[Authorize(Roles = "Admin")]
    public class DeliveryPartnerManagementController : Controller
    {
        private readonly string _connectionString = "Data Source=WIN-Q6UMJVN8K48\\SSMLUXRURYA;Initial Catalog=Foodie_demo_test;Persist Security Info=True;User ID=sa;Password=Admin@1234;Encrypt=False;Trust Server Certificate=True";

        // GET: Admin/DeliveryPartnerManagement
        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AdminLogin(string username, string password)
        {
            if (username == "admin" && password == "admin123")
            {
                HttpContext.Session.SetString("IsAdminLoggedIn", "true");
                return RedirectToAction("Dashboard"); // Redirect to dashboard after login
            }

            TempData["Error"] = "Invalid username or password.";
            return View();
        }

        public IActionResult AdminLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("AdminLogin");
        }

        public IActionResult Index()
        {
            // Create a list to store delivery partners
            var deliveryPartners = new List<DeliveryPartnerListViewModel>();

            // Query to fetch from database
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "SELECT partner_id, FullName, ContactNumber, Email, Status " +
                    "FROM deliverypartner.tbl_deliveryPartners", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        deliveryPartners.Add(new DeliveryPartnerListViewModel
                        {
                            PartnerId = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            ContactNumber = reader.GetInt64(2).ToString(),
                            Email = reader.GetString(3),
                            Status = reader.GetString(4)
                        });
                    }
                }
            }

            return View(deliveryPartners);
        }

        // POST: Admin/DeliveryPartnerManagement/ChangeStatus
        [HttpPost]
        public IActionResult ChangeStatus(int partnerId, string status)
        {
            if (partnerId <= 0 || string.IsNullOrEmpty(status))
            {
                TempData["Error"] = "Invalid partner ID or status";
                return RedirectToAction("Index");
            }

            // Validate that status is only allowed values
            if (status != "Active" && status != "Pending" && status != "Suspended")
            {
                TempData["Error"] = "Invalid status value";
                return RedirectToAction("Index");
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand(
                    "UPDATE deliverypartner.tbl_deliveryPartners SET Status = @Status WHERE partner_id = @PartnerId",
                    connection);
                command.Parameters.AddWithValue("@PartnerId", partnerId);
                command.Parameters.AddWithValue("@Status", status);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    TempData["Success"] = $"Partner status changed to {status} successfully";
                }
                else
                {
                    TempData["Error"] = "Failed to update partner status";
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult Profile(int id)
        {
            var profile = new DeliveryPartnerProfileViewModel();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Single query to get all partner data
                var query = @"
            SELECT 
                dp.partner_id,
                dp.FullName,
                dp.ContactNumber,
                dp.Email,
                dp.Status AS IsActive,
                dpd.ProfilePicture,
                dpd.AadhaarNumber,
                dpd.PANNumber,
                dd.AadhaarCard,
                dd.DrivingLicense,
                dd.PANCard,
                dd.RCBook,
                ppd.BankName,
                ppd.AccountNumber,
                ppd.IFSC_Code,
                dv.VehicleType,
                dv.LicensePlate,
                dv.RCNumber,
                dv.DrivingLicense AS VehicleDrivingLicense,
                dv.InsuranceDetails
            FROM [deliverypartner].[tbl_deliveryPartners] dp
            LEFT JOIN [deliverypartner].[tbl_deliveryPartnerDetails] dpd ON dp.partner_id = dpd.partner_id
            LEFT JOIN [deliverypartner].[tbl_deliveryDocuments] dd ON dp.partner_id = dd.partner_id
            LEFT JOIN [deliverypartner].[tbl_deliveryPartnerPaymentDetails] ppd ON dp.partner_id = ppd.partner_id
            LEFT JOIN [deliverypartner].[tbl_deliveryVehicle] dv ON dp.partner_id = dv.partner_id
            WHERE dp.partner_id = @PartnerId";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PartnerId", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        profile.PartnerId = reader.GetInt32(0);
                        profile.FullName = reader.IsDBNull(1) ? null : reader.GetString(1);
                        profile.ContactNumber =  reader.GetInt64(2);
                        profile.Email = reader.IsDBNull(3) ? null : reader.GetString(3);
                        profile.IsActive = !reader.IsDBNull(4) && reader.GetString(4) == "Active";
                        profile.ProfilePicture = reader.IsDBNull(5) ? null : reader.GetString(5);
                        profile.AadhaarNumber = reader.IsDBNull(6) ? null : reader.GetString(6);
                        profile.PANNumber = reader.IsDBNull(7) ? null : reader.GetString(7);
                        profile.AadhaarDoc = reader.IsDBNull(8) ? null : reader.GetString(8);
                        profile.LicenseDoc = reader.IsDBNull(9) ? null : reader.GetString(9);
                        profile.PANDoc = reader.IsDBNull(10) ? null : reader.GetString(10);
                        profile.VehicleDoc = reader.IsDBNull(11) ? null : reader.GetString(11);
                        profile.BankName = reader.IsDBNull(12) ? null : reader.GetString(12);
                        profile.AccountNumber = reader.IsDBNull(13) ? null : reader.GetString(13);
                        profile.IFSC_Code = reader.IsDBNull(14) ? null : reader.GetString(14);
                        profile.VehicleType = reader.IsDBNull(15) ? null : reader.GetString(15);
                        profile.LicensePlate = reader.IsDBNull(16) ? null : reader.GetString(16);

                    }
                }
            }

            return View(profile);
        }

        // You can keep these for backward compatibility or remove them
        public IActionResult Approve(int id)
        {
            return ChangeStatus(id, "Active");
        }

        public IActionResult Reject(int id)
        {
            return ChangeStatus(id, "Suspended");
        }

        public IActionResult Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                RestaurantEarnings = new List<EarningInfo>(),
                PartnerEarnings = new List<EarningInfo>()
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Get Total Partners
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM deliverypartner.tbl_deliveryPartners", connection))
                {
                    model.TotalPartners = (int)cmd.ExecuteScalar();
                }

                // Get Total Restaurants
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM vendores.tbl_restaurant", connection))
                {
                    model.TotalRestaurants = (int)cmd.ExecuteScalar();
                }

                // Get Total Customers
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM customers.tbl_customer", connection))
                {
                    model.TotalCustomers = (int)cmd.ExecuteScalar();
                }

                // Get Partner Earnings
                using (var cmd = new SqlCommand(@"
            SELECT 
                dp.FullName,
                ISNULL(SUM(de.Earnings), 0) AS TotalEarnings,
                ISNULL(w.Balance, 0) AS WalletBalance
            FROM 
                deliverypartner.tbl_deliveryPartners dp
            LEFT JOIN 
                deliverypartner.tbl_deliveryEarnings de ON dp.partner_id = de.partner_id
            LEFT JOIN 
                admins.tbl_wallet w ON dp.partner_id = w.partner_id
            GROUP BY 
                dp.FullName, w.Balance
            ", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            decimal totalEarning = reader.GetDecimal(1);
                            decimal walletBalance = reader.GetDecimal(2);

                            decimal finalAmount = totalEarning - walletBalance;

                            model.PartnerEarnings.Add(new EarningInfo
                            {
                                Name = reader.GetString(0),
                                TotalEarnings = finalAmount
                            });
                        }
                    }
                }

                // Get Restaurant Earnings (You can keep fake here for now or fetch real if tables available)
                model.RestaurantEarnings.Add(new EarningInfo { Name = "Pizza Hut", TotalEarnings = 56000 });
                model.RestaurantEarnings.Add(new EarningInfo { Name = "Burger King", TotalEarnings = 47000 });
                model.RestaurantEarnings.Add(new EarningInfo { Name = "Domino's", TotalEarnings = 63000 });
            }

            return View(model);
        }

        public IActionResult PartnerPerformance()
        {
            List<PartnerPerformanceModel> performanceList = new List<PartnerPerformanceModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_GetDeliveryPartnerPerformance", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    PartnerPerformanceModel model = new PartnerPerformanceModel
                    {
                        FullName = reader["FullName"].ToString(),
                        TotalOrdersDelivered = Convert.ToInt32(reader["TotalOrdersDelivered"]),
                        LateDeliveries = Convert.ToInt32(reader["LateDeliveries"]),
                        CancellationCount = Convert.ToInt32(reader["CancellationCount"]),
                        CustomerRatingAvg = Convert.ToDecimal(reader["CustomerRatingAvg"]),
                        UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                    };
                    performanceList.Add(model);
                }
                conn.Close();
            }
            return View(performanceList);
        }

        public IActionResult PartnerPerformanceGraphs()
        {
            List<PartnerPerformanceModel> performanceList = new List<PartnerPerformanceModel>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("admins.sp_GetDeliveryPartnerPerformance", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    PartnerPerformanceModel model = new PartnerPerformanceModel
                    {
                        FullName = reader["FullName"].ToString(),
                        TotalOrdersDelivered = Convert.ToInt32(reader["TotalOrdersDelivered"]),
                        LateDeliveries = Convert.ToInt32(reader["LateDeliveries"]),
                        CancellationCount = Convert.ToInt32(reader["CancellationCount"]),
                        CustomerRatingAvg = Convert.ToDecimal(reader["CustomerRatingAvg"]),
                        UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                    };
                    performanceList.Add(model);
                }
                conn.Close();
            }
            return View(performanceList);
        }

        // GET: AdminSettings
        [HttpGet]
        public IActionResult AdminSettings()
        {
            if (HttpContext.Session.GetString("IsAdminLoggedIn") != "true")
            {
                return RedirectToAction("AdminLogin");
            }

            // Populate static credentials
            var model = new AdminSettingViewModel
            {
                AdminEmail = "admin",
                AdminPassword = "admin123"
            };

            return View(model);
        }

        // POST: AdminSettings
        [HttpPost]
        public IActionResult AdminSettings(AdminSettingViewModel model)
        {
            if (HttpContext.Session.GetString("IsAdminLoggedIn") != "true")
            {
                return RedirectToAction("AdminLogin");
            }

            // Since credentials are static, you may just show a confirmation message for now
            TempData["Message"] = "Settings are currently static and cannot be saved. Future versions may support changes.";

            return View(model);
        }



    }
}