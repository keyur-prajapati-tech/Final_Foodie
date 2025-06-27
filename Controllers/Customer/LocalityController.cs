using Foodie.Models;
using Foodie.Models.customers;
using Foodie.Repositories;
using Foodie.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;

namespace Foodie.Controllers.Customer
{
    public class LocalityController : Controller
    {
        private readonly IcustomerRepository _repository;
        private const int DefaultPageSize = 5;

        public LocalityController(IcustomerRepository repository)
        {
            _repository = repository;
        }

        /*public IActionResult Locality(string DisrictName)
        {
            var cities = _repository.GetCitiesByDistrictName(DisrictName);
            var viewModel = new RegisterViewModel
            {
                Customer = new tbl_customer(),
                Cities = cities.ToList()
            };
            return View(viewModel);
        }*/

        public IActionResult Locality(string DisrictName)
        {
            var cities = _repository.GetCitiesByDistrictName(DisrictName);
            var email = HttpContext.Session.GetString("CustomerEmail");
            var name = HttpContext.Session.GetString("CustomerName");
            var customer = _repository.GetTbl_Customer(email);

            int restaurantId = 2;
            var topSellingMenus = _repository.topSellingMenuViewModels(restaurantId);

            string districtName = DisrictName;
            var model = new RegisterViewModel
            {
                Customer = customer,
                Cities = (List<tbl_city>)cities,
                topSellingMenus = topSellingMenus.ToList()

            };

            return View(model);
        }

        public IActionResult GetCustomerProfile()
        {
            var email = HttpContext.Session.GetString("CustomerEmail");
            var customer = _repository.GetTbl_Customer(email);
            if (customer != null)
            {
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        email = customer.email,
                        customer_name = customer.customer_name,
                        phone = customer.phone,
                        Gender = customer.Gender,
                        DOB = customer.DOB.ToString("yyyy-MM-dd")
                    }
                });
            }
            return Json(new { success = false, message = "Customer not found." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(tbl_customer updatedCustomer, IFormFile profilepic)
        {
            var email = HttpContext.Session.GetString("CustomerEmail");
            if (string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "User not logged in." });

            var existingCustomer = _repository.GetTbl_Customer(email);
            if (existingCustomer == null)
                return Json(new { success = false, message = "Customer not found." });

            byte[] profileImageBytes = null;
            if (profilepic != null && profilepic.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    profilepic.CopyTo(ms);
                    profileImageBytes = ms.ToArray();
                }
            }

            updatedCustomer.email = email; // prevent email change
            _repository.UpdateCustomerProfile(updatedCustomer, profileImageBytes);

            HttpContext.Session.SetString("CustomerName", updatedCustomer.customer_name);

            return Json(new { success = true, message = "Profile updated successfully!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult Register(tbl_customer customer, IFormFile profileimg)
        {
            if (customer == null || string.IsNullOrWhiteSpace(customer.email))
            {
                return Json(new { success = false, message = "Invalid user data." });
            }

            string email = customer.email.Trim().ToLower();
            var existing = _repository.GetTbl_Customer(email);

            byte[] profileimgBytes = null;
            if (profileimg != null && profileimg.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    profileimg.CopyTo(memoryStream);
                    profileimgBytes = memoryStream.ToArray();
                }
            }

            if (existing == null)
            {
                customer.email = email;
                _repository.AddUser(customer, profileimgBytes);
            }

            var nameToStore = existing?.customer_name ?? customer.customer_name ?? "User";
            HttpContext.Session.SetString("CustomerEmail", email);
            HttpContext.Session.SetString("UserName", nameToStore);
            HttpContext.Session.SetString("UserId", existing.customer_id.ToString());
            HttpContext.Session.SetString("UserEmail", "email");

            return Json(new
            {
                success = true,
                message = "Registered successfully!",
                redirectUrl = Url.Action("Locality", "Locality")
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new { success = false, message = "Email and password are required." });
            }

            // Attempt to authenticate as a customer
            var customer = _repository.ValidateCustomerLogin(email.Trim().ToLower(), password.Trim());
            if (customer != null)
            {
                HttpContext.Session.SetString("UserId", customer.customer_id.ToString());
                HttpContext.Session.SetString("UserName", customer.customer_name);
                HttpContext.Session.SetString("UserRole", "customer");
                HttpContext.Session.SetString("UserEmail", "email");

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("Locality", "Locality"),
                    message = "Login successful.",
                    userName = customer.customer_name
                });
            }

            // Attempt to authenticate as a restaurant
            var restaurant = _repository.ValidateRestaurantLogin(email.Trim().ToLower(), password.Trim());
            if (restaurant != null)
            {
                HttpContext.Session.SetString("UserId", restaurant.restaurant_id.ToString());
                HttpContext.Session.SetString("UserName", restaurant.restaurant_name);
                HttpContext.Session.SetString("UserRole", "restaurant");

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("reports", "OrderHistory"),
                    message = "Login successful.",
                    userName = restaurant.restaurant_name
                });
            }

            // Authentication failed
            return Json(new { success = false, message = "Invalid email or password." });
        }


        public IActionResult customerLayout()
        {
            var email = HttpContext.Session.GetString("CustomerEmail");
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Index", "Home"); // Or Login page
            }

            ViewBag.CustomerName = HttpContext.Session.GetString("CustomerName");
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult CustomerDashboard()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("CustomerEmail")))
            {
                return RedirectToAction("Index", "Home"); // or to Login
            }

            ViewBag.CustomerName = HttpContext.Session.GetString("CustomerName");
            return View();
        }

        public IActionResult GetAllOffers()
        {
            var offers = _repository.GetOffers();
            return View(offers);
        }

        public IActionResult OfferDetails(int offerId)
        {
            var offer = _repository.GetOfferById(offerId);
            if (offer == null)
            {
                return NotFound();
            }
            return RedirectToAction("offer","Locality");
        }

        public ActionResult specialOffers()
        {
            var offers = _repository.GetAllActiveOffers();
            return View(offers);
        }

        public JsonResult GetSpecialOffers(int page = 1, int pageSize = 6)
        {
            var allOffers = _repository.GetAllActiveOffers();
            var pagedOffers = allOffers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    o.so_id,
                    o.offer_title,
                    o.offer_desc,
                    o.ImagePath,
                    o.percentage_disc,
                    ValidFrom = o.validFrom.ToString("yyyy-MM-dd"),
                    ValidTo = o.validTo.ToString("yyyy-MM-dd")
                }).ToList();

            return Json(pagedOffers);
        }

        public async Task<IActionResult> GetNotifications(int? page, int? pageSize, string sortField, string sortDirection)
        {
            int currentPage = page ?? 1;
            int size = pageSize ?? DefaultPageSize;

            var model = _repository.GetCustomerFeedbacks(
                currentPage,
                size,
                sortField,
                sortDirection ?? "desc");

            return PartialView("_NotificationsPartial", model);
        }

        [HttpPost]
        public JsonResult AjaxCreate(tbl_customer_feedback feedback)
        {
            try
            { 
                _repository.InsertFeedback(feedback);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetApprovedRestaurants()
        {
            var restaurants = _repository.GetApprovedRestaurants();
            return Json(restaurants);
        }

        // GET: Cuisine
        [HttpGet]
        public IActionResult GetAllCuisines()
        {
            var cuisines = _repository.GetAllCuisines();
            return Json(cuisines);
        }

        [HttpPost]
        public IActionResult sendemailpage(OTPViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
                return Json(new { success = false, message = "Email is required." });

            string otp = GenerateOTP();
            HttpContext.Session.SetString("UserEmail", model.Email);
            _repository.SaveOTP(model.Email, otp);
            SendEmail(model.Email, otp);

            return Json(new { success = true, message = "OTP sent to your email.", email = model.Email });
        }

        private string GenerateOTP()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private void SendEmail(string toEmail, string otp)
        {
            var fromEmail = "keyurprajapati1402@gmail.com";
            var fromPassword = "lzcegjbyudtzxlvt";

            var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = "Your OTP Code",
                Body = $"<h3>Your OTP is: {otp}</h3><p>Valid for 1 minute only.</p>",
                IsBodyHtml = true
            };

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true
            };

            smtp.Send(message);
        }

        [HttpPost]
        public IActionResult Verify(string otp, string email)
        {
            if (string.IsNullOrEmpty(email))
                email = HttpContext.Session.GetString("UserEmail");

            if (_repository.isValidateOTP(email, otp))
            {
                return Json(new { success = true, message = "OTP verified successfully.", redirectUrl = Url.Action("Registerres", "Addrestaurant") });
            }
            else
            {
                return Json(new { success = false, message = "Invalid or expired OTP." });
            }
        }
    }
}
