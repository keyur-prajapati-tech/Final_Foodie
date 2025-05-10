using Foodie.Models.customers;
using Foodie.Repositories;
using Foodie.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Restaurant
{
    public class OrderHistoryController : Controller
    {
        private readonly IRestaurantRepository _restaurantRepository;

        public OrderHistoryController(IRestaurantRepository restaurantRepository)
        {
            _restaurantRepository = restaurantRepository;
        }

        public IActionResult OrdHistory()
        {
            var restaurantId = 1; // Replace with the actual restaurant ID

            var orderHistory = _restaurantRepository.tbl_Orders_History(restaurantId);

            return View(orderHistory);
        }

        

        public IActionResult offers()
        {
            return View();
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
