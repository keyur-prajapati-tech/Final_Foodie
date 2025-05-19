using Foodie.Models.Restaurant;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Foodie.Controllers.Restaurant
{
    [Route("Menu")]
    public class MenuController : Controller
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public MenuController(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        [Route("Add")]
        public IActionResult AddMenu()
        {
            int restaurant_id = 1; // Replace with actual restaurant ID


            return View();
        }

        [HttpGet]
        [Route("getMenuByRes")]
        public JsonResult getMenuByRes()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            var menu = _restaurantRepository.getMenuByRes(restaurantId);

            if (menu == null || menu.Count == 0)
            {
                return Json(new { success = false, message = "No menu items found." });
            }
           
           
            return Json(menu);

        }

        [HttpGet]
        [Route("getMenuById")]
        public JsonResult getMenuById(int menuId)
        {
            var menu = _restaurantRepository.getMenu(menuId);
            if (menu == null)
            {
                return Json(new { success = false, message = "Menu item not found." });
            }

            ViewBag.menuImg = Convert.ToBase64String(menu.menu_img);

            return Json(menu);
        }

        [HttpPost]
        [Route("DeleteMenu")]
        public JsonResult DeleteMenu(int menuId)
        {
            var result = _restaurantRepository.DeleteMenu(menuId);
            if (result > 0)
            {
                return Json(new { success = true, message = "Menu item deleted successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete menu item." });
            }
        }

        [HttpPost]
        [Route("UpdateMenu")]
        
        public async Task<IActionResult> UpdateMenu(tbl_menu_items menu, IFormFile food_img, string OldImage)
        {
            if (food_img != null && food_img.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await food_img.CopyToAsync(memoryStream);
                    menu.menu_img = memoryStream.ToArray();
                }
            }
            else if (!string.IsNullOrEmpty(OldImage))
            {
                // Use the old image from base64 string
                menu.menu_img = Convert.FromBase64String(OldImage);
            }

            var result = _restaurantRepository.UpdateMenu(menu);
            return RedirectToAction("Add");
        }


        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> AddMenu(tbl_menu_items menu, IFormFile food_img)
        {

            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            if (food_img != null && food_img.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await food_img.CopyToAsync(memoryStream);
                    menu.menu_img = memoryStream.ToArray();
                }
            }
            tbl_menu_items tbl_Menu_Items = new tbl_menu_items()
            {
                menu_name = menu.menu_name,
                cuisine_id = 1,
                menu_img = menu.menu_img,
                menu_descripation = menu.menu_descripation,
                amount = menu.amount,
                isAvailable = true,
                Restaurant_id = restaurantId
            };

            var data = _restaurantRepository.AddMenu(tbl_Menu_Items);

            return View();
        }

        [Route("InvDishes")]
        public IActionResult Dishes()
        {
            return View();
        }

        [Route("InvAddOns")]
        public IActionResult AddOns()
        {
            return View();
        }
    }
}
