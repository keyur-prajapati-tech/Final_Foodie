using Foodie.Models;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Foodie.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAdminRepository _AdminRepository;
        private readonly string _connectionstring;
        public LoginController(IAdminRepository adminRepository, IConfiguration configuration)
        {
            _AdminRepository = adminRepository;
            _connectionstring = configuration.GetConnectionString("defaultconnection");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string Email, string Password)
        {
            if (_AdminRepository.Login(Email, Password))
            {
                // Login successful

                //Session["UserEmail"] = ;


                var admin = _AdminRepository.getSessionData(Email); // This will return an employee object
                int role = admin.role_id; // Assuming Role is a string property in the 'employees' class
                int adminid = admin.admin_id; // Assuming Role is a string property in the 'employees' class
                byte[] imageBytes = admin.IMAGE;

                // Check if role is null or not before setting the session
                if (role != null)
                {
                    HttpContext.Session.SetString("role", role.ToString());
                    HttpContext.Session.SetString("admin_id", adminid.ToString());
                    HttpContext.Session.SetString("Email", Email);
                    HttpContext.Session.SetString("Image", Convert.ToBase64String(imageBytes));
                }
                else
                {
                    // Handle the case where role is not found, maybe log the issue
                    ViewBag.ErrorMessage = "User role not found.";
                    return View();
                }
                return RedirectToAction("DashBoard", "Orders");
            }
            else
            {
                // return RedirectToAction("DashBoard", "Orders");
                // Login failed
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View();
            }
        }
        public IActionResult home()
        {
            return View();
        }
        public IActionResult customers()
        {
            var customer = _AdminRepository.GetAllCustomer();
            return View(customer);
        }
        public IActionResult AddAdmin()
        {
            ViewBag.role = _AdminRepository.GetRole();
            return View(new tbl_admin());
        }
        [HttpPost]
        public async Task<IActionResult> AddAdmin(tbl_admin admin, IFormFile Image)
        {
            byte[] Image1 = null;
            if (Image != null && Image.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await Image.CopyToAsync(memoryStream);
                    Image1 = memoryStream.ToArray();
                }
            }
            bool res = _AdminRepository.AddAdmin(admin, Image1);
            if (res)
            {
                return RedirectToAction("Admin");
            }


            ViewBag.role = _AdminRepository.GetRole();

            return View(admin);
        }
        public IActionResult Admin()
        {
            ViewBag.role = _AdminRepository.GetRole();
            var admin = _AdminRepository.GetAll();
            return View(admin);
        }

        [HttpPost]

        public IActionResult UpdateProfile(IFormCollection form, IFormFile IMAGE)
        {
            int adminid = Convert.ToInt32(HttpContext.Session.GetString("admin_id"));
            var admin = _AdminRepository.GetAdminById(adminid);
            admin.Full_name = form["Full_name"];
            admin.Email = form["Email"];
            admin.Phone = form["Phone"];
            admin.admin_id = adminid;
            if (IMAGE != null && IMAGE.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    IMAGE.CopyTo(ms);
                    admin.IMAGE = ms.ToArray();
                }
            }
            else
            {
                // Reuse old image
                string oldImageBase64 = form["IMAGE"];
                if (!string.IsNullOrEmpty(oldImageBase64))
                {
                    admin.IMAGE = Convert.FromBase64String(oldImageBase64);
                }
            }

            _AdminRepository.UpdateAdmin(admin);

            HttpContext.Session.SetString("Email", admin.Email);
            HttpContext.Session.SetString("Image", Convert.ToBase64String(admin.IMAGE));
            return Ok();
        }

        public IActionResult GetProfile()
        {
            int adminId = Convert.ToInt32(HttpContext.Session.GetString("admin_id"));
            var admin = _AdminRepository.GetAdminById(adminId);

            var result = new
            {
                full_name = admin.Full_name,
                email = admin.Email,
                phone = admin.Phone,
                profilepic = admin.IMAGE != null ? Convert.ToBase64String(admin.IMAGE) : null
            };

            return Json(result);
        }

    }
}
