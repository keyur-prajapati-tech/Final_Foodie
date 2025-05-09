using Foodie.Repositories;
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
            return View();
        }

        public IActionResult Reviews()
        {
            var ratings = _restaurantRepository.GetAllRatings();
            return View(ratings);
        }

        public IActionResult OutletInfo()
        {
            var restaurantId = 2; // Replace with the actual restaurant ID

            var outletInfo = _restaurantRepository.getOutletInfo(restaurantId);

            return View(outletInfo);
        }
    }
}
