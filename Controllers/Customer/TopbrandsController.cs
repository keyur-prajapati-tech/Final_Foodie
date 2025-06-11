using Foodie.Models.customers;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Tls;

namespace Foodie.Controllers.Customer
{
    [Route("TBrands")]
    public class TopbrandsController : Controller
    {
        private readonly IcustomerRepository _repository;

        public TopbrandsController(IcustomerRepository repository)
        {
            _repository = repository;
        }

        [Route("")]
        [Route("Brands")]
        public IActionResult Brands()
        {
            var topbrands = _repository.GetTopRestaurant(5);
            return View(topbrands);
        }

        [Route("BrandDetails")]
        public IActionResult BrandDetails(string name)
        {
            var brand = _repository.GetRestaurantByName(name);
            if (brand == null)
            {
                return NotFound();
            }
            // Get menu items for this brand
            var menuItems = _repository.GetMenuItemsByRestaurantdetails(brand.RestaurantId);

            ViewBag.MenuItems = menuItems;

            return View(brand);
        }

        [HttpGet("GetBrandDetails")]
        public IActionResult GetBrandDetails(string brandName)
        {
            var brand = _repository.GetRestaurantByName(brandName);
            if (brand == null)
            {
                return Json(new { success = false, message = "Brand not found" });
            }
            return Json(new { success = true, data = brand });
        }

        // AJAX endpoint for similar restaurants
        public JsonResult GetSimilarRestaurants(string currentRestaurantName,
                                              List<string> cuisines,
                                              List<string> pincodes)
        {
            try
            {
                // Set defaults if empty
                if (cuisines == null || !cuisines.Any())
                    cuisines = new List<string> { "Chinese" };

                if (pincodes == null || !pincodes.Any())
                    pincodes = new List<string> { "395006" };

                var similarRestaurants = _repository.GetSimilarRestaurants(
                    currentRestaurantName,
                    cuisines,
                    pincodes);

                return Json(new
                {
                    success = true,
                    data = similarRestaurants
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error loading similar restaurants"
                });
            }
        }

        [HttpGet("GetMenuImages")]
        public IActionResult GetMenuImages(string restaurantName)
        {
            try
            {
                var menuImages = _repository.GetMenuItemsImageByRestaurant(restaurantName);

                if (menuImages == null || !menuImages.Any())
                {
                    return Json(new
                    {
                        success = true,
                        data = new List<object>(),
                        count = 0,
                        message = "No menu photos found"
                    });
                }

                // Convert byte arrays to base64 strings for the view
                var result = menuImages.Select(m => new {
                    menuId = m.MenuId,
                    menuName = m.MenuName,
                    imageBase64 = m.MenuImg != null ? Convert.ToBase64String(m.MenuImg) : null,
                    cuisineName = m.cuisine_name
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = result,
                    count = result.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error loading menu images"
                });
            }
        }


        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetMenuByRestaurant(string restaurantName)
        {
            var items = _repository.GetMenuItems(restaurantName);
            return Json(items);
        }

        [HttpGet]
        public JsonResult GetMenuByCuisine(string restaurantName, int cuisineId)
        {
            var items = _repository.GetMenuItemsByCuisine(restaurantName, cuisineId);
            return Json(items);
        }

        // Controller - Updated with more detailed response
        [HttpGet]
        public IActionResult GetRatings(string restaurantName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(restaurantName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Restaurant name is required",
                        receivedName = restaurantName // Debugging info
                    });
                }

                Console.WriteLine($"Fetching reviews for: {restaurantName}"); // Debug

                var ratings = _repository.GetRatingByRestaurant(restaurantName);

                Console.WriteLine($"Found {ratings?.Count ?? 0} reviews"); // Debug

                return Ok(new
                {
                    success = true,
                    data = ratings ?? new List<CustomerRatingViewModel>(),
                    count = ratings?.Count ?? 0,
                    isEmpty = (ratings == null || !ratings.Any())
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Controller error: {ex.Message}"); // Debug
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching ratings",
                    error = ex.Message
                });
            }
        }

        [HttpPost("SubmitRating")]
        public IActionResult SubmitRating([FromBody] CustomerRatingViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Invalid data",
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                    });
                }

                // Get current customer ID from session (you'll need to implement this)
                var customerId = HttpContext.Session.GetInt32("UserId");
                if (customerId == 0)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Please login to submit a review"
                    });
                }

                model.CustomerId = (int)customerId;

                bool result;
                if (model.RatingId > 0)
                {
                    // Update existing rating
                    result = _repository.UpdateRating(model);
                }
                else
                {
                    // Add new rating
                    result = _repository.AddRating(model);
                }

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = model.RatingId > 0 ? "Review updated successfully" : "Review added successfully"
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to save review"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while saving the review",
                    error = ex.Message
                });
            }
        }

        [HttpGet("GetUserRating")]
        public IActionResult GetUserRating(int restaurantId)
        {
            try
            {
                var customerId = HttpContext.Session.GetInt32("UserId");
                if (customerId == 0)
                {
                    return Ok(new
                    {
                        success = true,
                        hasRating = false
                    });
                }

                var rating = _repository.GetRatingById((int)customerId, restaurantId);
                if (rating == null)
                {
                    return Ok(new
                    {
                        success = true,
                        hasRating = false
                    });
                }

                return Ok(new
                {
                    success = true,
                    hasRating = true,
                    data = rating
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while fetching user rating",
                    error = ex.Message
                });
            }
        }
    }
}
