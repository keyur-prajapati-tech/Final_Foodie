using Foodie.Models.customers;
using Foodie.ViewModels;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;

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
            var restaurantId = 1; // Replace with the actual restaurant ID

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
            if (ModelState.IsValid)
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
            if (ModelState.IsValid)
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

        public IActionResult reports()
        {
            return View();
        }

        public IActionResult payouts()
        {
            return View();
        }


        public IActionResult complaint()
            {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            var vendor = _restaurantRepository.GetComplaintsByRestaurantId(restaurantId);
            //var customer = _AdminRepository.GetAllCustomerComplaints();
         
            
            return View(vendor);
        }

        [HttpPost]
        public IActionResult EditComplaint(tbl_cust_vendor_complaints tbl)
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            //var complaint = _AdminRepository.GetvendorcomByID(complaintId);
            //if (complaint == null)
            //{
            //    return NotFound();
            //}
            tbl.restaurant_id = restaurantId;
            _restaurantRepository.updateVencom(tbl);
            // Return a success response
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

            var outletInfo = _restaurantRepository.getOutletInfo(restaurantId);

            return View(outletInfo);
        }
    }
}
