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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IWebHostEnvironment _env;
        private readonly string _connStr;

        public DeliverSignup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _env = webHostEnvironment; // ✅ FIX: Assign the passed-in value to _env
            _connStr = configuration.GetConnectionString("DefaultConnection");
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

        [HttpPost]
        [Route("d/register")]
        public async Task<IActionResult> DeliveryRegister(tbl_deliveryPartners partner)
        {
            ViewData["Layout"] = "_DeliveryPartnerLayout";

            if (!ModelState.IsValid)
            {
                return View("DeliveryRegister", partner); // Return view with validation messages
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

                    // Call stored procedure to insert
                    using (SqlCommand cmd = new SqlCommand("sp_RegisterDeliveryPartner", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FullName", partner.FullName);
                        cmd.Parameters.AddWithValue("@ContactNumber", Convert.ToInt64(partner.ContactNumber));
                        cmd.Parameters.AddWithValue("@Email", partner.Email);
                        cmd.Parameters.AddWithValue("@Password", partner.Password);
                        cmd.Parameters.AddWithValue("@CreatedAT", DateTime.Now);

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
                                Status = reader["Status"].ToString()
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

            // 📁 Save profile image if uploaded
            if (profileImage != null && profileImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

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

                // 🔍 Check if details already exist
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

                // ✅ Insert details
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

                    // Now update tbl_deliveryPartners with new vehicle_id
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
            return RedirectToAction("PendingApproval");
        }

        [Route("d/dash")]
        public IActionResult Dashboard()
        {
            return View();
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
