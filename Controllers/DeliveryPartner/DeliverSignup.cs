using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;
using Foodie.Models;
using Microsoft.Extensions.Configuration;
using Foodie.Models.DeliveryPartner;

namespace Foodie.Controllers.DeliveryPartner
{
    [Route("deliverysignup")]
    public class DeliverSignup : Controller
    {
        private readonly IConfiguration _configuration;

        public DeliverSignup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }

        // 🎯 Delivery Partner Index Page
        [Route("d/index")]
        public IActionResult DeliveryIndex()
        {
            return View();
        }

        // 🎯 Delivery Partner Registration
        [Route("d/register")]
        public IActionResult DeliveryRegister()
        {
            ViewData["Layout"] = "_DeliveryPartnerLayout";
            return View("DeliveryRegister");
        }

        [HttpPost]
        [Route("d/register")]
        public async Task<IActionResult> DeliveryRegister(tbl_deliveryPartners partner)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        await conn.OpenAsync();
                        using (SqlCommand cmd = new SqlCommand("sp_RegisterDeliveryPartner", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@FullName", partner.FullName);
                            cmd.Parameters.AddWithValue("@ContactNumber", Convert.ToInt64(partner.ContactNumber));
                            cmd.Parameters.AddWithValue("@UserName", partner.UserName);
                            cmd.Parameters.AddWithValue("@Email", partner.Email);
                            cmd.Parameters.AddWithValue("@Password", partner.Password);
                            cmd.Parameters.AddWithValue("@CreatedAT", DateTime.Now);

                            int rowsAffected = await cmd.ExecuteNonQueryAsync();

                            if (rowsAffected > 0)
                            {
                                return RedirectToAction("DeliveryLogin");
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    ViewBag.Error = $"Database error: {ex.Message}";
                }
            }

            ViewBag.Error = "Registration failed. Please try again.";
            return View("DeliveryRegister");
        }

        // 🎯 Delivery Partner Login
        [Route("d/login")]
        public IActionResult DeliveryLogin()
        {
            return View("DeliveryLogin");
        }

        [HttpPost]
        [Route("d/login")]
        public async Task<IActionResult> DeliveryLogin(string Email, string Password)
        {
            var partner = await GetPartnerByEmailAndPasswordAsync(Email, Password);

            if (partner != null)
            {
                // ✅ Update LastLogin after successful login
                await UpdateLastLoginAsync(partner.partner_id);

                // ✅ Sign-in the user
                await SignInUser(partner);

                // ✅ Redirect based on the partner's status
                return HandleLoginRedirect(partner.Status);
            }

            // ❌ Invalid login, show error message
            TempData["Error"] = "Invalid email or password.";
            return RedirectToAction("DeliveryLogin");
        }

        // 🎯 Get partner by email and password
        [HttpGet]
        public async Task<tbl_deliveryPartners> GetPartnerByEmailAndPasswordAsync(string Email, string Password)
        {
            tbl_deliveryPartners partner = null;

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("sp_AuthDeliveryPartner", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Email", Email);
                    cmd.Parameters.AddWithValue("@Password", Password);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            partner = new tbl_deliveryPartners
                            {
                                partner_id = Convert.ToInt32(reader["partner_id"]),
                                UserName = reader["UserName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Status = reader["Status"].ToString()
                            };
                        }
                    }
                }
            }

            return partner;
        }

        // ✅ Update LastLogin after successful login
        private async Task UpdateLastLoginAsync(int partner_id)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                await conn.OpenAsync();

                using (SqlCommand cmd = new SqlCommand("UpdateLastLogin", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PartnerId", partner_id);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // 🎯 Sign-in user and create claims
        private async Task SignInUser(tbl_deliveryPartners partner)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, partner.Email),
                new Claim("PartnerId", partner.partner_id.ToString()),
                new Claim(ClaimTypes.Role, "DeliveryPartner")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            HttpContext.Session.SetInt32("PartnerId", partner.partner_id);
            HttpContext.Session.SetString("UserName", partner.UserName);
        }

        // 🎯 Handle login redirection based on status
        private IActionResult HandleLoginRedirect(string status)
        {
            if (status == "Inactive")
            {
                return RedirectToAction("Per_Details");
            }
            else if (status == "Active")
            {
                return RedirectToAction("Dashboard");
            }
            else if (status == "Pending")
            {
                return RedirectToAction("PendingApproval");
            }
            else
            {
                ViewBag.Error = "Your account is rejected. Please contact support.";
                return View("~/Views/DeliverSignup/DeliveryLogin.cshtml");
            }
        }

        private IActionResult ShowRejectedAccountError()
        {
            ViewBag.Error = "Your account is rejected. Please contact support.";
            return View("~/Views/DeliverSignup/DeliveryLogin.cshtml");
        }

        // 🎯 Pending Approval Page
        [HttpGet("d/details")]
        public IActionResult Per_Details()
        {
            var partnerId = HttpContext.Session.GetInt32("PartnerId");

            if (partnerId == null)
            {
                TempData["Error"] = "Please log in to continue.";
                return RedirectToAction("DeliveryLogin", "DeliverSignup");
            }

            tbl_deliveryPartnerDetails partnerDetails = null;

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetDeliveryPartnerDetailsById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@partner_id", partnerId);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            partnerDetails = new tbl_deliveryPartnerDetails
                            {
                                partner_id = Convert.ToInt32(reader["partner_id"]),
                                ProfilePicture = reader["ProfilePicture"].ToString(),
                                AadhaarNumber = reader["AadhaarNumber"].ToString(),
                                PANNumber = reader["PANNumber"].ToString()
                            };
                        }
                    }
                }
            }

            if (partnerDetails == null)
            {
                TempData["Message"] = "Please enter your personal details.";
                return View("~/Views/DeliverSignup/Per_Details.cshtml", new tbl_deliveryPartnerDetails());
            }

            return View("~/Views/DeliverSignup/Per_Details.cshtml", partnerDetails);
        }

        [HttpPost("d/details")]
        public IActionResult Per_Details(tbl_deliveryPartnerDetails per)
        {
            var partnerId = HttpContext.Session.GetInt32("PartnerId");

            if (partnerId == null)
            {
                TempData["Error"] = "Your session has expired. Please log in again.";
                return RedirectToAction("DeliveryLogin");
            }

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                con.Open();

                // Check if details already exist
                using (SqlCommand checkCmd = new SqlCommand("sp_GetDeliveryPartnerDetailsById", con))
                {
                    checkCmd.CommandType = CommandType.StoredProcedure;
                    checkCmd.Parameters.AddWithValue("@partner_id", partnerId);

                    using (SqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            TempData["Error"] = "Your personal details already exist. You can update them from your profile.";
                            return RedirectToAction("BankDetails");
                        }
                    }
                }

                // Insert new details
                using (SqlCommand cmd = new SqlCommand("sp_InsertDeliveryPartnerDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@partner_id", partnerId);
                    cmd.Parameters.AddWithValue("@ProfilePicture", per.ProfilePicture);
                    cmd.Parameters.AddWithValue("@AadhaarNumber", per.AadhaarNumber);
                    cmd.Parameters.AddWithValue("@PANNumber", per.PANNumber);

                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Success"] = "Details saved successfully. Proceed to enter your vehicle details.";
            return RedirectToAction("VehicleDetails");
        }



        // 🎯 Get All Delivery Partner Details
        [HttpGet("all")]
        public IActionResult GetAllDetails()
        {
            List<tbl_deliveryPartnerDetails> ProfileList = new List<tbl_deliveryPartnerDetails>();

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllDeliveryPartnerDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ProfileList.Add(new tbl_deliveryPartnerDetails
                            {
                                detail_id = Convert.ToInt32(reader["detail_id"]),
                                partner_id = Convert.ToInt32(reader["partner_id"]),
                                ProfilePicture = reader["ProfilePicture"].ToString(),
                                AadhaarNumber = reader["AadhaarNumber"].ToString(),
                                PANNumber = reader["PANNumber"].ToString()
                            });
                        }
                    }
                    con.Close();
                }
            }

            return View("~/Views/DeliverSignup/Profile.cshtml", ProfileList);
        }

        // 🎯 Update Delivery Partner Details
        [HttpPost("update")]
        public IActionResult UpdateDetails(tbl_deliveryPartnerDetails model)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_UpdateDeliveryPartnerDetails", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@detail_id", model.detail_id);
                        cmd.Parameters.AddWithValue("@ProfilePicture", model.ProfilePicture ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@AadhaarNumber", model.AadhaarNumber);
                        cmd.Parameters.AddWithValue("@PANNumber", model.PANNumber);

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();

                        TempData["Success"] = "Details updated successfully!";
                        return RedirectToAction("GetAllDetails");
                    }
                }
            }

            return View("~/Views/DeliverSignup/Per_Details.cshtml", model);
        }

        // 🎯 Delete Delivery Partner Details
        [HttpPost("delete/{id}")]
        public IActionResult DeleteDetails(int id)
        {
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteDeliveryPartnerDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@detail_id", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                    TempData["Success"] = "Details deleted successfully!";
                }
            }

            return RedirectToAction("GetAllDetails");
        }


        // 🎯 Additional CRUD Pages

        [HttpGet("d/vehicle")]
        public IActionResult VehicleDetails() {
            return View();
        }

        [HttpPost("d/vehicle")]
        public IActionResult VehicleDetails(tbl_deliveryVehicle vehicle)
        {
            if (vehicle == null || vehicle.partner_id == 0)
            {
                TempData["Error"] = "Invalid vehicle data.";
                return RedirectToAction("VehicleDetails", new { partner_id = vehicle?.partner_id ?? 0 });
            }

            try
            {
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_InsertVehicle", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Use DBNull.Value if the value is null or empty
                        cmd.Parameters.AddWithValue("@partner_id", vehicle.partner_id);
                        cmd.Parameters.AddWithValue("@RCNumber", vehicle.RCNumber);
                        cmd.Parameters.AddWithValue("@VehicleType", vehicle.VehicleType);
                        cmd.Parameters.AddWithValue("@LicensePlate",vehicle.LicensePlate);
                        cmd.Parameters.AddWithValue("@MakeModel", vehicle.MakeModel);
                        cmd.Parameters.AddWithValue("@DrivingLicense", vehicle.DrivingLicense);
                        cmd.Parameters.AddWithValue("@InsuranceDetails", vehicle.InsuranceDetails);

                        con.Open();
                        cmd.ExecuteNonQuery();
                        
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
            }
            return RedirectToAction("Document", new { partner_id = vehicle.partner_id });
        }

        [HttpPost("d/Update")]
        public IActionResult UpdateVehicle(tbl_deliveryVehicle vehicle)
        {
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateVehicle", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@vehicle_id", vehicle.vehicle_id);
                    cmd.Parameters.AddWithValue("@RCNumber", vehicle.RCNumber);
                    cmd.Parameters.AddWithValue("@VehicleType", vehicle.VehicleType);
                    cmd.Parameters.AddWithValue("@LicensePlate", vehicle.LicensePlate);
                    cmd.Parameters.AddWithValue("@MakeModel", vehicle.MakeModel);
                    cmd.Parameters.AddWithValue("@DrivingLicense", vehicle.DrivingLicense);
                    cmd.Parameters.AddWithValue("@InsuranceDetails", vehicle.InsuranceDetails);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = "Vehicle details updated successfully.";
            return RedirectToAction("VehicleDetails", new { partner_id = vehicle.partner_id });
        }

        [HttpPost]
        public IActionResult DeleteVehicle(int vehicle_id)
        {
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteVehicle", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@vehicle_id", vehicle_id);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = "Vehicle details deleted successfully.";
            return RedirectToAction("VehicleDetails");
        }

        [HttpGet("d/bank")]
        public IActionResult BankDetails()
        {
            int partnerId = HttpContext.Session.GetInt32("PartnerId") ?? 0;
            if (partnerId == 0)
            {
                throw new Exception("partnerId is 0 or not set in session.");
            }

            tbl_deliveryPartnerPaymentDetails paymentDetail = null;

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand("SP_GetBankDetailsByPartnerId", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@partner_id", SqlDbType.Int).Value = partnerId; // FIXED PARAMETER NAME

                con.Open();
                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read()) // Fetch only one record
                {
                    paymentDetail = new tbl_deliveryPartnerPaymentDetails
                    {
                        PaymentDetailID = Convert.ToInt32(rdr["PaymentDetailID"]),
                        partner_id = Convert.ToInt32(rdr["partner_id"]),
                        PaymentType = rdr["PaymentType"].ToString(),
                        UPI_ID = rdr["UPI_ID"].ToString(),
                        BankName = rdr["BankName"].ToString(),
                        AccountNumber = rdr["AccountNumber"].ToString(),
                        IFSC_Code = rdr["IFSC_Code"].ToString(),
                        IsDefault = Convert.ToBoolean(rdr["IsDefault"])
                    };
                }
            }

            return View(paymentDetail); // Pass a single object, not a list
        }

        [HttpPost("d/bank")]
        public IActionResult BankDetails(tbl_deliveryPartnerPaymentDetails model)
        {
            int? partnerId = HttpContext.Session.GetInt32("PartnerId");
            if (partnerId == null)
            {
                TempData["ErrorMessage"] = "Session expired. Please log in again.";
                return RedirectToAction("DeliveryLogin");
            }

            model.partner_id = partnerId.Value;

            if (model.PaymentDetailID > 0)
            {
                UpdateBankDetails(model);
            }
            else
            {
                InsertBankDetails(model);
            }

            TempData["SuccessMessage"] = "Bank details saved successfully!";
            return RedirectToAction("BankDetails"); // Reload the page after saving
        }

        // 🟢 Separate Method to Insert Bank Details
        private void InsertBankDetails(tbl_deliveryPartnerPaymentDetails model)
        {
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("sp_InsertBankDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@partner_id", model.partner_id);
                    cmd.Parameters.AddWithValue("@PaymentType", model.PaymentType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UPI_ID", model.UPI_ID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BankName", model.BankName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AccountNumber", model.AccountNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IFSC_Code", model.IFSC_Code ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsDefault", model.IsDefault);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // 🟢 Separate Method to Update Bank Details
        private void UpdateBankDetails(tbl_deliveryPartnerPaymentDetails model)
        {
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("sp_UpdateBankDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PaymentDetailID", model.PaymentDetailID);
                    cmd.Parameters.AddWithValue("@partner_id", model.partner_id);
                    cmd.Parameters.AddWithValue("@PaymentType", model.PaymentType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@UPI_ID", model.UPI_ID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BankName", model.BankName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@AccountNumber", model.AccountNumber ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IFSC_Code", model.IFSC_Code ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@IsDefault", model.IsDefault);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // 🟢 Delete Bank Details
        [HttpPost]
        public IActionResult DeleteBankDetails(int paymentDetailId, int partnerId)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_DeleteBankDetails", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PaymentDetailID", paymentDetailId);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("BankDetails", new { partnerId = partnerId });
        }


        [Route("d/doc")]
        public IActionResult Document()
        {
            return View();
        }

        [Route("d/dash")]
        public IActionResult Dashboard()
        {
            return View();
        }

        // 🎯 Admin Side: Time Slots Page
        [Route("d/slots")]
        public IActionResult TimeSlot()
        {
            return View();
        }

        [Route("d/support")]
        public IActionResult Support()
        {
            return View();
        }

        [HttpGet("d/profile")]
        public IActionResult Profile()
        {
            var partnerId = HttpContext.Session.GetInt32("PartnerId");

            if (partnerId == null)
            {
                TempData["Error"] = "Please log in to view your profile.";
                return RedirectToAction("DeliveryLogin");
            }

            tbl_deliveryPartnerDetails partnerDetails = null;

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetDeliveryPartnerDetailsById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@partner_id", partnerId);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            partnerDetails = new tbl_deliveryPartnerDetails
                            {
                                detail_id = Convert.ToInt32(reader["detail_id"]),
                                partner_id = Convert.ToInt32(reader["partner_id"]),
                                ProfilePicture = reader["ProfilePicture"].ToString(),
                                AadhaarNumber = reader["AadhaarNumber"].ToString(),
                                PANNumber = reader["PANNumber"].ToString()
                            };
                        }
                    }
                }
            }

            if (partnerDetails == null)
            {
                TempData["Error"] = "No details found! Please complete your personal details.";
                return RedirectToAction("Per_Details");
            }

            return View("Profile", partnerDetails);
        }

    }
}
