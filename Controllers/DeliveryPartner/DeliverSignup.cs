using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;
using Foodie.Models;
using Foodie.Models.DeliveryPartner;
using Microsoft.Extensions.Configuration;
using Foodie.Models.DeliveryPartner;
using Foodie.Models.Admin;
using Foodie.ViewModels;
using Foodie.Repositories;




namespace Foodie.Controllers.DeliveryPartner
{
    [Route("deliverysignup")]
    public class DeliverSignup : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWebHostEnvironment _env;
        private readonly string _connStr;

        private readonly IDeliveryPatnerRepository _deliveryPatnerRepository;

        public DeliverSignup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment, IDeliveryPatnerRepository deliveryPatnerRepository)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _env = webHostEnvironment;
            _connStr = configuration.GetConnectionString("DefaultConnection");
            _deliveryPatnerRepository = deliveryPatnerRepository;
        }


        private string GetConnectionString()
        {
            return _configuration.GetConnectionString("DefaultConnection");
        }

        [Route("d/index")]
        public IActionResult DeliveryIndex()
        {
            return View();
        }

        [Route("d/register")]
        public IActionResult DeliveryRegister()
        {
            ViewData["Layout"] = "_DeliveryPartnerLayout";
            return View("DeliveryRegister");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("d/login");
        }

       [HttpPost]
        [Route("d/register")]
        public async Task<IActionResult> DeliveryRegister(tbl_deliveryPartners partner)
        {
            ViewData["Layout"] = "_DeliveryPartnerLayout";

            if (!ModelState.IsValid)
            {

                //await CheckAndUpdatePartnerStatusAsync(partner.partner_id);

                return View("DeliveryRegister", partner);
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                {
                    await conn.OpenAsync();

                    // Check if email already exists
                    using (SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM deliverypartner.tbl_deliveryPartners WHERE Email = @Email", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", partner.Email);
                        int emailExists = (int)await checkCmd.ExecuteScalarAsync();

                        if (emailExists > 0)
                        {
                            ModelState.AddModelError("Email", "This email is already registered.");
                            return View("DeliveryRegister", partner);
                        }
                    }

                    // Call stored procedure to register
                    using (SqlCommand cmd = new SqlCommand("sp_RegisterDeliveryPartner", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@FullName", partner.FullName);
                        cmd.Parameters.AddWithValue("@ContactNumber", partner.ContactNumber); // already string (nvarchar)
                        cmd.Parameters.AddWithValue("@Email", partner.Email);
                        cmd.Parameters.AddWithValue("@Password", partner.Password);

                        // These are optional or default values set in SP
                        cmd.Parameters.AddWithValue("@Status", "Inactive");         // or "Offline"
                        cmd.Parameters.AddWithValue("@CreatedAT", DateTime.Now);
                        cmd.Parameters.AddWithValue("@isApprov", 0);
                        cmd.Parameters.AddWithValue("@isOnline", 0);
                        cmd.Parameters.AddWithValue("@Isavailable", 0);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            TempData["Success"] = "Registration successful. Please wait for admin approval.";
                            return RedirectToAction("DeliveryLogin");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", $"Database error: {ex.Message}");
            }

            ModelState.AddModelError("", "Registration failed. Please try again.");
            return View("DeliveryRegister", partner);
        }

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
                //await CheckAndUpdatePartnerStatusAsync(partner.partner_id);

                await UpdateLastLoginAsync(partner.partner_id);
                await SignInUser(partner);
                return HandleLoginRedirect(partner.Status);
            }

            TempData["Error"] = "Invalid email or password.";
            return RedirectToAction("DeliveryLogin");
        }

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
                                Email = reader["Email"].ToString(),
                                FullName = reader["FullName"].ToString(),
                                Status = reader["Status"].ToString(),
                                isOnline = Convert.ToBoolean(reader["isOnline"]),
                                isApprov = Convert.ToBoolean(reader["isApprov"])
                            };
                        }
                    }
                }
            }

            return partner;
        }


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

        private async Task SignInUser(tbl_deliveryPartners partner)
        {
            HttpContext.Session.SetInt32("PartnerId", partner.partner_id);
            HttpContext.Session.SetString("FullName", partner.FullName ?? "");
            HttpContext.Session.SetInt32("isOnline", partner.isOnline ? 1 : 0);
            HttpContext.Session.SetInt32("isApprov", partner.isApprov ? 1 : 0);
            //HttpContext.Session.SetString("Status", partner.Status);
        }


        private IActionResult HandleLoginRedirect(string status)
        {
            switch (status)
            {
                case "Inactive":
                    return RedirectToAction("Per_Details");
                case "Active":
                    return RedirectToAction("Dashboard");
                case "Pending":
                    return RedirectToAction("PendingApproval");
                default:
                    return ShowRejectedAccountError();
            }
        }

        private IActionResult ShowRejectedAccountError()
        {
            ViewBag.Error = "Your account is rejected. Please contact support.";
            return View("~/Views/DeliverSignup/DeliveryLogin.cshtml");
        }

        [HttpGet("d/details")]
        public IActionResult Per_Details()
        {
            var partnerId = HttpContext.Session.GetInt32("PartnerId");

            if (partnerId == null)
            {
                TempData["Error"] = "Please log in to continue.";
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
        public async Task<IActionResult> Per_Details(tbl_deliveryPartnerDetails per, IFormFile profileImage)
        {
            var partnerId = HttpContext.Session.GetInt32("PartnerId");

            if (partnerId == null)
            {
                TempData["Error"] = "Your session has expired. Please log in again.";
                return RedirectToAction("DeliveryLogin");
            }

            string uniqueFileName = null;

            if (profileImage != null && profileImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsFolder);

                string fileExtension = Path.GetExtension(profileImage.FileName);
                uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(fileStream);
                }

                per.ProfilePicture = uniqueFileName;
            }

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                con.Open();


                bool detailsExist = false;

                using (SqlCommand checkCmd = new SqlCommand("sp_GetDeliveryPartnerDetailsById", con))
                {
                    checkCmd.CommandType = CommandType.StoredProcedure;
                    checkCmd.Parameters.AddWithValue("@partner_id", partnerId);

                    using (SqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            detailsExist = true;
                        }
                    }
                }

                if (detailsExist)
                {
                    TempData["Error"] = "Your personal details already exist. You can update them from your profile.";
                    return RedirectToAction("BankDetails");
                }


                using (SqlCommand cmd = new SqlCommand("sp_InsertDeliveryPartnerDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@partner_id", partnerId);
                    cmd.Parameters.AddWithValue("@ProfilePicture", per.ProfilePicture ?? "");
                    cmd.Parameters.AddWithValue("@AadhaarNumber", per.AadhaarNumber);
                    cmd.Parameters.AddWithValue("@PANNumber", per.PANNumber);

                    cmd.ExecuteNonQuery();
                }

            }

            TempData["Success"] = "Details saved successfully. Proceed to enter your vehicle details.";
            return RedirectToAction("VehicleDetails");
        }





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




        [HttpGet("d/vehicle")]
        public IActionResult VehicleDetails()
        {
            return View();
        }

        [HttpPost("d/vehicle")]
        public IActionResult VehicleDetails(tbl_deliveryVehicle vehicle)
        {
            var partnerId = HttpContext.Session.GetInt32("PartnerId");

            if (vehicle == null || partnerId == null)
            {
                TempData["Error"] = "Invalid vehicle data.";
                return RedirectToAction("VehicleDetails");
            }

            try
            {
                int newVehicleId = 0;

                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_InsertVehicle", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@partner_id", partnerId);
                        cmd.Parameters.AddWithValue("@RCNumber", vehicle.RCNumber ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@VehicleType", vehicle.VehicleType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@LicensePlate", vehicle.LicensePlate ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@MakeModel", vehicle.MakeModel ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@DrivingLicense", vehicle.DrivingLicense ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@InsuranceDetails", vehicle.InsuranceDetails ?? (object)DBNull.Value);

                        SqlParameter outputParam = new SqlParameter("@NewVehicleID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputParam);

                        con.Open();
                        cmd.ExecuteNonQuery();

                        newVehicleId = Convert.ToInt32(outputParam.Value);
                    }


                    using (SqlCommand updateCmd = new SqlCommand("UPDATE [deliverypartner].[tbl_deliveryPartners] SET vehicle_id = @vehicle_id WHERE partner_id = @partner_id", con))
                    {
                        updateCmd.Parameters.AddWithValue("@vehicle_id", newVehicleId);
                        updateCmd.Parameters.AddWithValue("@partner_id", partnerId);
                        updateCmd.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Vehicle details saved successfully.";
                return RedirectToAction("BankDetails", new { partner_id = partnerId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                return RedirectToAction("VehicleDetails");
            }
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
            return RedirectToAction("Document"); // Reload the page after saving
        }


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


        [HttpGet("d/doc")]
        public IActionResult Document()
        {
            return View();
        }

        [HttpPost("d/doc")]
        public IActionResult Document(IFormFile AadhaarCard, IFormFile PANCard, IFormFile DrivingLicense, IFormFile RCBook, IFormFile ProfilePhoto)
        {
            int? partnerId = HttpContext.Session.GetInt32("PartnerId");
            if (partnerId == null)
            {
                TempData["Error"] = "Session expired. Please log in again.";
                return RedirectToAction("Login");
            }

            string UploadFile(IFormFile file)
            {
                if (file != null && file.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var uploadPath = Path.Combine(_env.WebRootPath, "Uploads", "Documents");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);
                    var fullPath = Path.Combine(uploadPath, fileName);
                    using (var fs = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(fs);
                    }
                    return "/Uploads/Documents/" + fileName;
                }
                return null;
            }

            string aadhaarPath = UploadFile(AadhaarCard);
            string panPath = UploadFile(PANCard);
            string licensePath = UploadFile(DrivingLicense);
            string rcPath = UploadFile(RCBook);
            string photoPath = UploadFile(ProfilePhoto);

            bool isExisting = false;
            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("sp_CheckDeliveryDocumentExists", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@partner_id", partnerId.Value);
                int count = (int)cmd.ExecuteScalar();
                isExisting = count > 0;
            }

            using (SqlConnection conn = new SqlConnection(_connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(isExisting ? "sp_UpdateDeliveryDocuments" : "sp_InsertDeliveryDocuments", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@partner_id", partnerId.Value);
                cmd.Parameters.AddWithValue("@AadhaarCard", (object?)aadhaarPath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PANCard", (object?)panPath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DrivingLicense", (object?)licensePath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RCBook", (object?)rcPath ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = isExisting ? "Documents updated successfully!" : "Documents uploaded successfully!";
            return RedirectToAction("PaySignupFee");
        }

        [Route("d/dash")]
        public IActionResult Dashboard()
        {
            int partnerId = HttpContext.Session.GetInt32("PartnerId") ?? 0;
            if (partnerId == 0) return RedirectToAction("Login", "Account");

            int isOnline = HttpContext.Session.GetInt32("isOnline") ?? 0;

            var model = new DashViewModel
            {
                OrdersDelivered = _deliveryPatnerRepository.GetTodayDeliveriesAsync(partnerId),
                TotalEarnings = _deliveryPatnerRepository.GetTodayEarningsAsync(partnerId),
                AvgDeliveryTime = _deliveryPatnerRepository.GetAverageDeliveryTimeAsync(partnerId),
                AvgRating = _deliveryPatnerRepository.GetAverageRatingAsync(partnerId),
                LatestOrder = _deliveryPatnerRepository.GetLatestOrderAsync(partnerId),
                WeeklyLabels = new List<string> { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" },
                WeeklyEarnings = new List<decimal> { 200, 300, 150, 400, 450, 500, 350 },
                OrderStatusLabels = new List<string> { "Delivered", "In Transit", "Cancelled" },
                OrderStatusCounts = new List<int> { 12, 3, 1 }
            };

            return View("Dashboard", model);
        }

        [HttpPost]
        public IActionResult UpdateOnlineStatus(bool isOnline)
        {
            int partnerId = HttpContext.Session.GetInt32("PartnerId") ?? 0;
            if (partnerId == 0) return Unauthorized();

            try
            {
                bool success = _deliveryPatnerRepository.UpdateOnlineStatus(partnerId, isOnline);
                if (!success) return BadRequest("Failed to update status");

                HttpContext.Session.SetInt32("isOnline", isOnline ? 1 : 0);
                return Ok(new { isOnline });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            int partnerId = HttpContext.Session.GetInt32("PartnerId") ?? 0;
            if (partnerId == 0) return Unauthorized();

            try
            {
                bool success = _deliveryPatnerRepository.UpdateOrderStatus(orderId, status);
                return success
                    ? Ok(new { success = true, message = "Order status updated successfully" })
                    : BadRequest(new { success = false, message = "Failed to update order status" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error updating order status: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReportProblem(int orderId, string description)
        {
            int partnerId = HttpContext.Session.GetInt32("PartnerId") ?? 0;
            if (partnerId == 0) return Unauthorized();

            try
            {
                using (var connection = new SqlConnection(_connStr))
                {
                    await connection.OpenAsync();

                    // Insert problem report
                    var insertQuery = @"INSERT INTO deliverypartner.tbl_problem_reports 
                    (order_id, partner_id, description, reported_at, status)
                    VALUES (@OrderId, @PartnerId, @Description, GETDATE(), 'Pending')";

                    using (var cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", orderId);
                        cmd.Parameters.AddWithValue("@PartnerId", partnerId);
                        cmd.Parameters.AddWithValue("@Description", description);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Update order status
                    var updateQuery = @"UPDATE customers.tbl_orders
                    SET order_status = 'waiting'
                    WHERE order_id = @OrderId";

                    using (var cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@OrderId", orderId);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    return Ok(new { success = true, message = "Problem reported successfully" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Error reporting problem: {ex.Message}" });
            }
        }
        //public IActionResult Dashboard()
        //{
        //    int partnerId = HttpContext.Session.GetInt32("PartnerId") ?? 0;
        //    int isOnline = HttpContext.Session.GetInt32("isOnline") ?? 0;

        //    var model = new DashViewModel();


        //    using (SqlConnection conn = new SqlConnection(_connStr))
        //    {
        //        conn.Open();

        //        // ✅ Today Summary
        //        SqlCommand todayCmd = new SqlCommand("deliverypartner.sp_GetTodaySummary", conn);
        //        todayCmd.CommandType = CommandType.StoredProcedure;
        //        todayCmd.Parameters.AddWithValue("@PartnerId", partnerId);
        //        using (SqlDataReader reader = todayCmd.ExecuteReader())
        //        {
        //            if (reader.Read())
        //            {
        //                model.OrdersDelivered = reader.GetInt32(0);
        //                model.TotalEarnings = reader.GetDecimal(1);
        //            }
        //        }

        //        // ✅ Weekly Earnings
        //        SqlCommand weekCmd = new SqlCommand("deliverypartner.sp_GetWeeklyEarnings", conn);
        //        weekCmd.CommandType = CommandType.StoredProcedure;
        //        weekCmd.Parameters.AddWithValue("@PartnerId", partnerId);
        //        using (SqlDataReader reader = weekCmd.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                model.WeeklyLabels.Add(reader.GetString(0));
        //                model.WeeklyEarnings.Add(reader.GetDecimal(1));
        //            }
        //        }

        //        // ✅ Order Status Breakdown
        //        SqlCommand statusCmd = new SqlCommand("deliverypartner.sp_GetOrderStatusSummary", conn);
        //        statusCmd.CommandType = CommandType.StoredProcedure;
        //        statusCmd.Parameters.AddWithValue("@PartnerId", partnerId);
        //        using (SqlDataReader reader = statusCmd.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                model.OrderStatusLabels.Add(reader.GetString(0));
        //                model.OrderStatusCounts.Add(reader.GetInt32(1));
        //            }
        //        }

        //        conn.Close();
        //    }

        //    using (SqlConnection con = new SqlConnection(_connStr))
        //    {
        //        using (SqlCommand cmd = new SqlCommand("deliverypartner.sp_GetAssignedOrders", con))
        //        {
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.AddWithValue("@PartnerId", partnerId); // Use the partnerId from session
        //            con.Open();

        //            using (SqlDataReader rdr = cmd.ExecuteReader())
        //            {
        //                if (rdr.Read()) // Get the first assigned order
        //                {
        //                    model.RestaurantLat = rdr["restaurant_lat"].ToString();
        //                    model.RestaurantLng = rdr["restaurant_lag"].ToString();
        //                    model.CustomerLat = rdr["latitude"].ToString();
        //                    model.CustomerLng = rdr["longitude"].ToString();
        //                }
        //            }
        //        }
        //    }

        //    // ✅ Fetch latest delivery request status (to show/hide customer)
        //    using (SqlConnection conn = new SqlConnection(_connStr))
        //    {
        //        conn.Open();
        //        SqlCommand cmd = new SqlCommand(@"
        //    SELECT TOP 1 RequestStatus
        //    FROM deliverypartner.tbl_deliveryRequest
        //    ORDER BY request_id DESC", conn);

        //        var status = cmd.ExecuteScalar()?.ToString() ?? "Pending";
        //        ViewBag.RequestStatus = status;
        //    }

        //    // 🔁 Optional fallback dummy data
        //    model.OrdersDelivered = 7;
        //    model.TotalEarnings = 2350.50m;
        //    model.WeeklyLabels = new List<string> { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
        //    model.WeeklyEarnings = new List<decimal> { 200, 300, 150, 400, 450, 500, 350 };
        //    model.OrderStatusLabels = new List<string> { "Delivered", "In Transit", "Cancelled" };
        //    model.OrderStatusCounts = new List<int> { 12, 3, 1 };

        //    return View("Dashboard", model);
        //}



        [HttpPost]
        [Route("d/toggle-status")]
        public IActionResult ToggleStatusForm()
        {
            int? partnerId = HttpContext.Session.GetInt32("PartnerId");
            if (partnerId == null) return RedirectToAction("DeliveryLogin");

            int currentStatus = HttpContext.Session.GetInt32("isOnline") ?? 0;
            int newStatus = currentStatus == 1 ? 0 : 1;

            HttpContext.Session.SetInt32("isOnline", newStatus);

            using (SqlConnection con = new SqlConnection(_connStr))
            {
                string query = "UPDATE deliverypartner.tbl_deliveryPartners SET isOnline = @status WHERE partner_id = @id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@status", newStatus);
                cmd.Parameters.AddWithValue("@id", partnerId);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            return RedirectToAction("Dashboard");
        }


        [HttpGet("d/GetAvailableRestaurants")]
        public IActionResult GetAvailableRestaurants()
        {
            var restaurants = new List<dynamic>();

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("deliverypartner.GetAvailableRestaurants", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            restaurants.Add(new
                            {
                                restaurant_id = rdr["restaurant_id"],
                                restaurant_name = rdr["restaurant_name"].ToString(),
                                restaurant_lat = rdr["restaurant_lat"].ToString(),
                                restaurant_lag = rdr["restaurant_lag"].ToString()
                            });
                        }
                    }
                }
            }
            return Json(restaurants);
        }

        [Route("d/pending")]
        public IActionResult PendingApproval()
        {
            return View();
        }

        [Route("d/support")]
        public IActionResult Support()
        {
            return View();
        }
        [Route("d/terms")]
        public IActionResult Terms()
        {
            return View();
        }

        [HttpGet("d/profile")]
        public IActionResult Profile()
        {
            int? partnerId = HttpContext.Session.GetInt32("PartnerId");
            if (partnerId == null)
            {
                TempData["Error"] = "Please log in to access your profile.";
                return RedirectToAction("DeliveryLogin");
            }

            var model = new DeliveryPartnerProfileViewModel
            {
                PartnerId = partnerId.Value
            };

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                con.Open();

                // 1. Personal Details
                using (SqlCommand cmd = new SqlCommand("sp_GetDeliveryPartnerDetailsById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@partner_id", partnerId.Value);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.DetailId = Convert.ToInt32(reader["detail_id"]);
                            model.AadhaarNumber = reader["AadhaarNumber"].ToString();
                            model.PANNumber = reader["PANNumber"].ToString();
                            model.ProfilePicture = reader["ProfilePicture"].ToString();
                        }
                    }
                }

                // 2. Vehicle Info
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM deliverypartner.tbl_deliveryVehicle WHERE partner_id = @partner_id", con))
                {
                    cmd.Parameters.AddWithValue("@partner_id", partnerId.Value);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.VehicleType = reader["VehicleType"].ToString();
                            model.LicensePlate = reader["LicensePlate"].ToString();
                        }
                    }
                }

                // 3. Bank Info
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM deliverypartner.tbl_deliveryPartnerPaymentDetails WHERE partner_id = @partner_id", con))
                {
                    cmd.Parameters.AddWithValue("@partner_id", partnerId.Value);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.BankName = reader["BankName"].ToString();
                            model.AccountNumber = reader["AccountNumber"].ToString();
                            model.IFSC_Code = reader["IFSC_Code"].ToString();
                        }
                    }
                }
            }

            return View("Profile", model);
        }

        [HttpPost("d/profile")]
        public async Task<IActionResult> UpdateProfile(DeliveryPartnerProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid data submitted.";
                return View("Profile", model);
            }

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                await con.OpenAsync();

                // 1. Update Personal Details
                string updateDetailsQuery = @"
            IF EXISTS (SELECT 1 FROM deliverypartner.tbl_deliveryPartnerDetails WHERE partner_id = @partner_id)
            UPDATE deliverypartner.tbl_deliveryPartnerDetails
            SET AadhaarNumber = @AadhaarNumber, PANNumber = @PANNumber
            WHERE partner_id = @partner_id
            ELSE
            INSERT INTO deliverypartner.tbl_deliveryPartnerDetails (partner_id, AadhaarNumber, PANNumber)
            VALUES (@partner_id, @AadhaarNumber, @PANNumber)";
                using (SqlCommand cmd = new SqlCommand(updateDetailsQuery, con))
                {
                    cmd.Parameters.AddWithValue("@partner_id", model.PartnerId);
                    cmd.Parameters.AddWithValue("@AadhaarNumber", model.AadhaarNumber ?? "");
                    cmd.Parameters.AddWithValue("@PANNumber", model.PANNumber ?? "");
                    await cmd.ExecuteNonQueryAsync();
                }

                // 2. Update Vehicle Info
                string updateVehicleQuery = @"
            IF EXISTS (SELECT 1 FROM deliverypartner.tbl_deliveryVehicle WHERE partner_id = @partner_id)
            UPDATE deliverypartner.tbl_deliveryVehicle
            SET VehicleType = @VehicleType, LicensePlate = @LicensePlate
            WHERE partner_id = @partner_id
            ELSE
            INSERT INTO deliverypartner.tbl_deliveryVehicle (partner_id, VehicleType, LicensePlate)
            VALUES (@partner_id, @VehicleType, @LicensePlate)";
                using (SqlCommand cmd = new SqlCommand(updateVehicleQuery, con))
                {
                    cmd.Parameters.AddWithValue("@partner_id", model.PartnerId);
                    cmd.Parameters.AddWithValue("@VehicleType", model.VehicleType ?? "");
                    cmd.Parameters.AddWithValue("@LicensePlate", model.LicensePlate ?? "");
                    await cmd.ExecuteNonQueryAsync();
                }

                // 3. Update Bank Info
                string updateBankQuery = @"
            IF EXISTS (SELECT 1 FROM deliverypartner.tbl_deliveryPartnerPaymentDetails WHERE partner_id = @partner_id)
            UPDATE deliverypartner.tbl_deliveryPartnerPaymentDetails
            SET BankName = @BankName, AccountNumber = @AccountNumber, IFSCCode = @IFSCCode
            WHERE partner_id = @partner_id
            ELSE
            INSERT INTO deliverypartner.tbl_deliveryPartnerPaymentDetails (partner_id, BankName, AccountNumber, IFSCCode)
            VALUES (@partner_id, @BankName, @AccountNumber, @IFSCCode)";
                using (SqlCommand cmd = new SqlCommand(updateBankQuery, con))
                {
                    cmd.Parameters.AddWithValue("@partner_id", model.PartnerId);
                    cmd.Parameters.AddWithValue("@BankName", model.BankName ?? "");
                    cmd.Parameters.AddWithValue("@AccountNumber", model.AccountNumber ?? "");
                    cmd.Parameters.AddWithValue("@IFSCCode", model.IFSC_Code ?? "");
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }


        [HttpGet("d/fees")]
        public IActionResult PaySignupFee()
        {
            if (HttpContext.Session.GetInt32("PartnerId") == null)
            {
                TempData["Error"] = "Session expired. Please login again.";
                return RedirectToAction("DeliveryLogin", "DeliveryAccount");
            }
            return View();
        }

        [HttpPost("d/fees")]
        public IActionResult PaySignupFee(string transactionId)
        {
            int? partnerId = HttpContext.Session.GetInt32("PartnerId");
            if (partnerId == null || string.IsNullOrEmpty(transactionId))
            {
                TempData["Error"] = "Invalid payment data.";
                return RedirectToAction("PaySignupFee");
            }

            try
            {
                using (SqlConnection con = new SqlConnection(GetConnectionString()))
                {
                    // Check if already paid
                    string checkQuery = "SELECT COUNT(*) FROM admins.tbl_signupFees WHERE partner_id = @partner_id AND Status = 'Completed'";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@partner_id", partnerId);
                        con.Open();
                        int count = (int)checkCmd.ExecuteScalar();
                        con.Close();

                        if (count > 0)
                        {
                            TempData["Error"] = "You have already paid the signup fee.";
                            return RedirectToAction("d/pending");
                        }
                    }

                    // Insert payment record
                    string insertQuery = @"INSERT INTO admins.tbl_signupFees 
                (partner_id, Amount, PaymentMethod, TransactionID, PaidAt, Status) 
                VALUES (@partner_id, @Amount, @PaymentMethod, @TransactionID, @PaidAt, @Status)";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@partner_id", partnerId);
                        cmd.Parameters.AddWithValue("@Amount", 1200); // ₹1200
                        cmd.Parameters.AddWithValue("@PaymentMethod", "UPI");
                        cmd.Parameters.AddWithValue("@TransactionID", transactionId);
                        cmd.Parameters.AddWithValue("@PaidAt", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Status", "Completed");
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand command = new SqlCommand(
                         "UPDATE deliverypartner.tbl_deliveryPartners SET Status = @Status WHERE partner_id = @PartnerId",
                         con))
                    {


                        command.Parameters.AddWithValue("@PartnerId", partnerId);
                        command.Parameters.AddWithValue("@Status", "Pending");

                        command.ExecuteNonQuery();
                    }



                }

                TempData["Success"] = "Payment successful!";

                return RedirectToAction("PendingApproval");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error occurred: " + ex.Message;
                return RedirectToAction("PaySignupFee");
            }
        }

        private async Task CheckAndUpdatePartnerStatusAsync(int partnerId)
        {

            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("sp_CheckAndUpdateStatus", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PartnerId", partnerId);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }


        public IActionResult FeeStatus()
        {
            int? partnerId = HttpContext.Session.GetInt32("PartnerId");
            if (partnerId == null)
            {
                TempData["Error"] = "Session expired.";
                return RedirectToAction("DeliveryLogin", "DeliveryAccount");
            }

            tbl_signupFees fee = new tbl_signupFees();

            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                string query = "SELECT TOP 1 * FROM admins.tbl_signupFees WHERE partner_id = @partner_id ORDER BY PaidAt DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@partner_id", partnerId);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        fee.partner_id = Convert.ToInt32(reader["partner_id"]);
                        fee.Amount = Convert.ToDecimal(reader["Amount"]);
                        fee.PaymentMethod = reader["PaymentMethod"].ToString();
                        fee.TransactionID = reader["TransactionID"].ToString();
                        fee.Status = reader["Status"].ToString();
                        fee.PaidAt = Convert.ToDateTime(reader["PaidAt"]);
                    }
                }
            }

            return View(fee);

        }
        public DashViewModel GetTodayDeliverySummary(int partnerId)
        {
            DashViewModel summary = new DashViewModel();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                SqlCommand cmd = new SqlCommand("deliverypartner.sp_GetTodayDeliverySummary", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PartnerId", partnerId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    summary.OrdersDelivered = Convert.ToInt32(reader["OrdersDelivered"]);
                    summary.AvgDeliveryTime = Convert.ToInt32(reader["AvgDeliveryTime"]);
                    summary.TotalEarnings = Convert.ToDecimal(reader["TotalEarnings"]);
                }
                conn.Close();
            }

            return summary;
        }
    }
}
