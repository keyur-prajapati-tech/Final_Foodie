using System.Data;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Foodie.Controllers
{
    public class AddRestaurantController : Controller
    {
        private readonly IAdminRepository _AdminRepository;

        public AddRestaurantController(IAdminRepository adminRepository)
        {
            _AdminRepository = adminRepository;
        }
        public IActionResult AddRestaurant()
        {

            
            return View();



            //return new EmptyResult();
            //return Challenge();
            //return SignOut("AuthenticationScheme");
            //return Content("Hello, Students welcome to SSM Lec");
            //return Redirect("https://example.com");
            //return RedirectToAction("Index", "Home");
            //return RedirectToRoute("Index");
            //return LocalRedirect("/Home/Index");
            //return Ok();
            //return Ok(new { message = "Success" });
            //return NoContent();
            //return BadRequest();
            //return BadRequest("Invalid data");
            //return Unauthorized();
            //return Forbid();
            //return NotFound();
            //return NotFound(new { message = "Resource not found" });
            //return Conflict();
            //return Conflict("Conflict occurred");
            //return Json(new { id = 1, name = "Example" });

        }
        public IActionResult Restaurant()
        {
            var vendor = _AdminRepository.GetAllRestaurant();
            return View(vendor);
        }

       

        public IActionResult AddCusine()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetCuisines()
        {
            return Json(_AdminRepository.GetAllCuisines());
        }

        [HttpPost]
        public JsonResult AddCuisine(string cuisineName)
        {
            if (string.IsNullOrWhiteSpace(cuisineName))
                return Json(new { success = false });

            int id = _AdminRepository.AddCuisine(cuisineName);
            return Json(new { success = true, id });
        }

        [HttpPost]
        public JsonResult UpdateCuisine(int id, string cuisineName)
        {
            _AdminRepository.UpdateCuisine(id, cuisineName);
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult DeleteCuisine(int id)
        {
            _AdminRepository.DeleteCuisine(id);
            return Json(new { success = true });
        }
        [HttpGet]
        public IActionResult GetPendingRestaurants()
        {
            var restaurants = _AdminRepository.GetPendingRestaurant();
            return Ok(restaurants);
        }
    }
}
