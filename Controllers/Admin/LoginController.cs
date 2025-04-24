using food_Demo.Models;
using food_Demo.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace food_Demo.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAdminRepository _AdminRepository;

        public LoginController (IAdminRepository adminRepository)
        {
            _AdminRepository = adminRepository;
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

                // Check if role is null or not before setting the session
                if (role != null)
                {
                    HttpContext.Session.SetString("role", role.ToString());
                    HttpContext.Session.SetString("admin_id", adminid.ToString());
                    HttpContext.Session.SetString("Email", Email);
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
            return View();
        }
        public IActionResult AddAdmin()
        {
            ViewBag.role = _AdminRepository.GetRole();
            return View(new tbl_admin());
        }
        [HttpPost]
        public async Task<IActionResult> AddAdmin(tbl_admin admin,IFormFile Image)
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

       
    }
}
