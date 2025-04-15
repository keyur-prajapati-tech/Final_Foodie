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

        public IActionResult Locality(string DisrictName)
        {
            var cities = _repository.GetCitiesByDistrictName(DisrictName);
            return View(cities);
        }

        [HttpPost]
        public IActionResult UpdateCustomer(tbl_customer customer,IFormFile profileimg)
        {
            byte[] profileimg1 = null;
            if (profileimg1 != null && profileimg.Length > 0)
            {
                using(var memoryStream = new MemoryStream())
                {
                    profileimg.CopyTo(memoryStream);
                    profileimg1 = memoryStream.ToArray();
                }
            }

            bool result = _repository.updateProfile(customer, profileimg1);

            if (result)
            {
                return RedirectToAction("Locality");
            }
            Console.WriteLine("Modal Validation failed:");
            foreach(var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            return View(customer);
        }

        //[HttpPost]
        //public JsonResult GoogleLogin([FromBody] tbl_customer customer)
        //{
        //    try
        //    {
        //        if(customer == null || string.IsNullOrWhiteSpace(customer.email))
        //        {
        //            return Json(new { success = false, message = "Invalid user data." });
        //        }
        //        var email = customer.email.Trim().ToLower();
        //        var existingUser = _repository.GetTbl_Customer(email);


        //        if(existingUser == null)
        //        {
        //            customer.email = email;

        //            _repository.AddUser(customer);
        //        }

        //        HttpContext.Session.SetString("email", email);
        //        HttpContext.Session.SetString("name", customer.name ?? existingUser?.name);
        //        HttpContext.Session.SetString("UID", customer.UID);

        //        return Json(new
        //        {
        //            success = true,
        //            redirectUrl = Url.Action("Locality", "Locality"),
        //            message = "Logged In Successfully...!"
        //        });
        //    }catch(Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = "Google sign-in failed",
        //            error = ex.Message
        //        });
        //    }
        //}
        [HttpPost]
        public JsonResult GoogleLogin([FromBody] tbl_customer customer)
        {
            try
            {
                if (customer == null || string.IsNullOrWhiteSpace(customer.email))
                {
                    return Json(new { success = false, message = "Invalid user data." });
                }

                string email = customer.email.Trim().ToLower();
                var existing = _repository.GetTbl_Customer(email);

                if (existing == null)
                {
                    customer.email = email;
                    _repository.AddUser(customer);
                }

                HttpContext.Session.SetString("CustomerEmail", email);
                HttpContext.Session.SetString("CustomerName", customer.name ?? existing?.name);
                HttpContext.Session.SetString("CustomerUID", customer.UID);

                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("Index", "Home"),
                    message = "Login successful."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error", error = ex.Message });
            }
        }

    }
}
