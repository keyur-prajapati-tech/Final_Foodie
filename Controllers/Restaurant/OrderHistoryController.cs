using Foodie.Models.Restaurant;
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

        

        public IActionResult offers(string SearchTerm)
        {
            var offers = string.IsNullOrEmpty(SearchTerm) ? _restaurantRepository.Get_Special_Offers() : _restaurantRepository.special_offer_search(SearchTerm);
            return View(offers);
        }

        public IActionResult AddOffers()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddOffers(tbl_special_offers offermodel) 
        { 
            if(ModelState.IsValid)
            {
                if(offermodel.OfferImage != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(offermodel.OfferImage.FileName);
                    string extension = Path.GetExtension(offermodel.OfferImage.FileName);
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    offermodel.ImagePath = "~/images/Offers/" + fileName;
                    fileName = Path.Combine(_env.WebRootPath, "images/Offers", fileName);
                    using (var fileStream = new FileStream(fileName, FileMode.Create))
                    {
                        offermodel.OfferImage.CopyTo(fileStream);
                    }
                }
                _restaurantRepository.Add_SP_Offer(offermodel);
                return RedirectToAction("offers");
            }
            return View(offermodel);
        }

        public IActionResult EditOffers(int id)
        {
            var offer = _restaurantRepository.Get_Special_Offers_ById(id);
            return View(offer);
        }

        [HttpPost]
        public IActionResult EditOffers(tbl_special_offers offermodel)
        {
            if (ModelState.IsValid)
            {
                if (offermodel.OfferImage != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(offermodel.OfferImage.FileName);
                    string extension = Path.GetExtension(offermodel.OfferImage.FileName);
                    fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    offermodel.ImagePath = "~/images/Offers/" + fileName;
                    fileName = Path.Combine(_env.WebRootPath, "images/Offers", fileName);
                    using (var fileStream = new FileStream(fileName, FileMode.Create))
                    {
                        offermodel.OfferImage.CopyTo(fileStream);
                    }
                }
                _restaurantRepository.Update_SP_offer(offermodel);
                return RedirectToAction("offers");
            }
            return View(offermodel);
        }

        public IActionResult DeleteOffers(int id)
        {
            _restaurantRepository.Delete_SP_offer(id);
            return RedirectToAction("offers");
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
            return View();
        }

        public IActionResult OutletInfo()
        {
            var restaurantId = 1; // Replace with the actual restaurant ID

            var outletInfo = _restaurantRepository.getOutletInfo(restaurantId);

            return View(outletInfo);
        }
    }
}
