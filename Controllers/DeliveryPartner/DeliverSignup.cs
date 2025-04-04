using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;
using Foodie.Models;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ViewEngines;

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
            if (!ModelState.IsValid)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("sp_RegisterDeliveryPartner", conn);


                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FullName", partner.FullName);
                        cmd.Parameters.AddWithValue("@ContactNumber", Convert.ToInt64(partner.ContactNumber));
                        cmd.Parameters.AddWithValue("@Email", partner.Email);
                        cmd.Parameters.AddWithValue("@Password", partner.Password);
                        cmd.Parameters.AddWithValue("@CreatedAT", DateTime.Now);
                       

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
            return View("~/Views/DeliverSignup/DeliveryLogin.cshtml");
        }

        [HttpPost]
        [Route("d/login")]
        public async Task<IActionResult> DeliveryLogin(string email, string password)
        {
            // Check if the user exists in the database
            var user = GetPartnerByEmailAndPassword(email, password);

            if (user != null)
            {
                // ✅ Update LastLogin after successful login
                UpdateLastLogin(user.Partner_Id);

                // Redirect to appropriate page after login
                return RedirectToAction("PendingApproval");
            }
            else
            {
                // Invalid login, show error message
                TempData["Error"] = "Invalid email or password.";
                return RedirectToAction("DeliveryLogin");
            }
        }
        // Method to get partner by email and password
        private tbl_deliveryPartners GetPartnerByEmailAndPassword(string email, string password)
        {
            tbl_deliveryPartners partner = null;
            using (SqlConnection conn = new SqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_AuthDeliveryPartner", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            partner = new tbl_deliveryPartners
                            {
                                Partner_Id = Convert.ToInt32(reader["partner_id"]),
                                FullName = reader["FullName"].ToString(),
                                Email = reader["Email"].ToString(),
                                Status = reader["Status"].ToString()
                            };
                        }
                    }
                }
            }
            return partner;
        }

        // ✅ Method to update LastLogin after successful login
        private void UpdateLastLogin(int partnerId)
        {
            using (SqlConnection con = new SqlConnection(GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand("UpdateLastLogin", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PartnerId", partnerId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Sign-in user and create claims
        private async Task SignInUser(tbl_deliveryPartners partner)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, partner.Email),
        new Claim("PartnerId", partner.Partner_Id.ToString()),
        new Claim(ClaimTypes.Role, "DeliveryPartner")
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        private IActionResult HandleLoginRedirect(string status)
        {
            if (status == "Pending")
            {
                return RedirectToAction("PendingApproval");
            }
            else if (status == "Approved")
            {
                return RedirectToAction("Per_Details");
            }
            else
            {
                ViewBag.Error = "Your account is rejected. Please contact support.";
                return View("~/Views/DeliverSignup/DeliveryLogin.cshtml");
            }
        }


        //  Pending Approval Page

        [Route("d/pending")]
        public IActionResult PendingApproval()
        {
            return View("~/Views/DeliverSignup/PendingApproval.cshtml");
        }

        //  Additional Details Page

        [Route("d/details")]
        public IActionResult Per_Details()
        {
            return View("~/Views/DeliverSignup/Per_Details.cshtml");
        }

        //  Additional CRUD Pages

        [Route("d/vehicle")]
        public IActionResult VehicleDetails()
        {
            return View();
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


        //this method will be in admin side
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

        [Route("d/profile")]
        public IActionResult Profile()
        {
            return View();
        }
    }
}
