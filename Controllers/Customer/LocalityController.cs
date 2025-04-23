using Foodie.Models.customers;
using Foodie.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Customer
{
    public class LocalityController : Controller
    {
        private readonly IcustomerRepository _repository;

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
            var customer = _repository.GetTbl_Customer(email);

            string districtName = DisrictName;
            var model = new RegisterViewModel
            {
                Customer = customer,
                Cities = (List<tbl_city>)cities
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
        public IActionResult Register(tbl_customer customer, IFormFile profileimg)
        {
            if (customer == null || string.IsNullOrWhiteSpace(customer.email))
            {
                return Json(new { success = false, message = "Invalid user data." });
            }

            // Normalize email
            string email = customer.email.Trim().ToLower();

            // Check if customer exists
            var existing = _repository.GetTbl_Customer(email);

            // Convert profile image to byte array
            byte[] profileimgBytes = null;
            if (profileimg != null && profileimg.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    profileimg.CopyTo(memoryStream);
                    profileimgBytes = memoryStream.ToArray();
                }
            }

            // Register if not existing
            if (existing == null)
            {
                customer.email = email;
                _repository.AddUser(customer, profileimgBytes);
            }

            // Use either new customer or existing one to set session
            var nameToStore = existing?.customer_name ?? customer.customer_name;

            HttpContext.Session.SetString("CustomerEmail", email);
            HttpContext.Session.SetString("CustomerName", nameToStore);

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Locality", "Locality"),
                message = "Login successful."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new { success = false, message = "Email and password are required." });
            }

            var customer = _repository.ValidateCustomerLogin(email.Trim().ToLower(),password.Trim().ToLower());

            if (customer == null || customer.password != password)
            {
                return Json(new { success = false, message = "Invalid email or password." });
            }

            HttpContext.Session.SetString("CustomerEmail", customer.email);
            HttpContext.Session.SetString("CustomerName", customer.customer_name);

            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Locality", "Locality"),
                message = "Login successful.",
                customerName = customer.customer_name
            });
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
    }
}
