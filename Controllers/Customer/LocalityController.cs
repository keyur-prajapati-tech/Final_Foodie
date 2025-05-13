using Foodie.Models.customers;
using Foodie.Repositories;
using Microsoft.AspNetCore.Authorization;
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
            HttpContext.Session.SetString("CustomerName", nameToStore);
            HttpContext.Session.SetString("customer_id", existing.customer_id.ToString());

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

    }
}
