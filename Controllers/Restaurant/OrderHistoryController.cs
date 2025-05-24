using Foodie.Models.customers;
using Foodie.ViewModels;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;
using Foodie.Models.Restaurant;

namespace Foodie.Controllers.Restaurant
{
    public class OrderHistoryController : Controller
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IWebHostEnvironment _env;

        public OrderHistoryController(IRestaurantRepository restaurantRepository, IWebHostEnvironment env)
        {
            _restaurantRepository = restaurantRepository;
            _env = env;
        }

        public IActionResult OrdHistory()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId")); ; // Replace with the actual restaurant ID

            var orderHistory = _restaurantRepository.tbl_Orders_History(restaurantId);

            return View(orderHistory);
        }


        public IActionResult offers()
        {
            var offers = _restaurantRepository.GetAllOffers();
            return View(offers);
        }

        [HttpPost]
        public async Task<IActionResult> AddOffer(tbl_special_offers offer, List<IFormFile> image_path)
        {
            if (!ModelState.IsValid)
            {
                string imagePaths = "";

                if (image_path != null && image_path.Count > 0)
                {
                    foreach (var file in image_path)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(_env.WebRootPath, "Uploads", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        imagePaths += "/Uploads/" + fileName + ",";
                    }

                    imagePaths = imagePaths.TrimEnd(',');
                    offer.ImagePath = imagePaths;
                }

                _restaurantRepository.AddOffeer(offer);
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Validation failed" });
        }


        [HttpGet]
        public IActionResult GetOffer(int id)
        {
            var offer = _restaurantRepository.GetOfferById(id);

            if(offer == null)
            {
                return NotFound();
            }
            return Json(offer);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOffer(tbl_special_offers offer, List<IFormFile> image_path)
        {
            if (!ModelState.IsValid)
            {
                string imagePaths = "";

                if (image_path != null && image_path.Count > 0)
                {
                    foreach (var file in image_path)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(_env.WebRootPath, "Uploads", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        imagePaths += "/Uploads/" + fileName + ",";
                    }

                    imagePaths = imagePaths.TrimEnd(',');

                    offer.ImagePath = imagePaths;
                }

                _restaurantRepository.UpdateOffer(offer);
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Validation Failed" });
        }

        [HttpPost]
        public IActionResult deleteOffer(int id)
        {
            _restaurantRepository.DeleteOffer(id);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult SearchOffers(DateTime? validFrom, DateTime? validTo)
        {
            var offers = _restaurantRepository.GetOffersByDateRange(validFrom, validTo);
            return View("offers", offers);
        }

        public IActionResult reports()
        {
            return View();
        }

        public IActionResult payouts()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId")); // Or wherever you're storing it
            var viewModel = _restaurantRepository.GetBankDetailsByRestaurantId(restaurantId);

            if (viewModel == null || viewModel.BankDetails == null)
            {
                // Optional: handle empty state (no bank details found)
                ViewBag.Message = "No bank details found.";
            }

            return View(viewModel);
        }
        [HttpGet]
        public IActionResult GetBadOrderStats()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            var stats = _restaurantRepository.GetWeeklyOrderStatsAsync(restaurantId);
            return Json(stats);
        }
        [HttpGet]
        public  IActionResult GetWeeklyRatings()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            var data =  _restaurantRepository.GetWeeklyCustomerRatings(restaurantId);
            return Json(data);
        }
        public IActionResult complaint()
            {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            var vendor = _restaurantRepository.GetComplaintsByRestaurantId(restaurantId);
            return View(vendor);
        }

        [HttpGet]
        public IActionResult GetPerformanceMetrics()
        {
            var result = _restaurantRepository.GetPerformanceMetrics();
            return Json(result);
        }

        [HttpPost]
        public IActionResult EditComplaint(tbl_cust_vendor_complaints tbl)
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            tbl.restaurant_id = restaurantId;
            _restaurantRepository.updateVencom(tbl);
            return RedirectToAction("complaint");
        }
        public IActionResult Reviews()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            var ratings = _restaurantRepository.GetAllRatings(restaurantId);
            return View(ratings);
        }

        public IActionResult OutletInfo()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId")); // Replace with the actual restaurant ID
            var model = _restaurantRepository.GetOutletInfo(restaurantId);
            return View(model);
        }
        [HttpPost]
        public IActionResult EditOutletInfo(OutletInfo model)
        {
            if (model.NewRestaurantImg != null)
            {
                using var ms = new MemoryStream();
                model.NewRestaurantImg.CopyTo(ms);
                model.restaurant_img = ms.ToArray();
            }

            if (model.NewRestaurantMenuImg != null)
            {
                using var ms = new MemoryStream();
                model.NewRestaurantMenuImg.CopyTo(ms);
                model.restaurant_menu_img = ms.ToArray();
            }

            _restaurantRepository.UpdateOutletInfo(model);
            TempData["Success"] = "Outlet information updated successfully.";
            return RedirectToAction("OutletInfo", new { id = model.restaurant_id });
        }

    }
}
