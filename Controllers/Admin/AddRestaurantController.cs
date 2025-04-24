using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers
{
    public class AddRestaurantController : Controller
    {
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
            return View();
        }

        public IActionResult Res()
        {
            return View();
        }
        public IActionResult AddCusine()
        {
            return View();
        }
    }
}
