using Foodie.Models.Restaurant;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Restaurant
{
    [Route("AddRestaurant")]
    public class AddrestaurantController : Controller
    {
        private readonly IRestaurantRepository _repository;

        public AddrestaurantController(IRestaurantRepository repository)
        {
            _repository = repository;
        }

        [Route("Registerres")]
        public IActionResult Registerres()
        {

            int r_id = RestaurantRepository.getRId();

            if (r_id != 0)
            {
                RestaurantRegistrationViewModel model = new RestaurantRegistrationViewModel
                {

                    tbl_Restaurant = _repository.getRestaurant(r_id),
                    tbl_Owner = _repository.getOwner(r_id)
                };
                return View(model);

            }
            else
            {
                return View();
            }
        }

        [Route("ResDocument")]
        public IActionResult ResDocument()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RegisterRestaurnt(RestaurantRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var owner = new tbl_owner_details
                {
                    owner_name = model.tbl_Owner.owner_name,
                    owner_email = model.tbl_Owner.owner_email,
                    owner_contact = model.tbl_Owner.owner_contact
                };

                int ownerid = _repository.AddOwner(owner);

                var restaurant = new tbl_restaurant
                {
                    restaurant_name = model.tbl_Restaurant.restaurant_name,
                    restaurant_contact = model.tbl_Owner.owner_contact,
                    restaurant_email = model.tbl_Owner.owner_email,
                    restaurant_street = model.tbl_Restaurant.restaurant_street,
                    restaurant_pincode = model.tbl_Restaurant.restaurant_pincode,
                    restaurant_lat = "dfg",
                    restaurant_lag = "ddgbfg",
                    restaurant_isActive = true,
                    est_id = 1,
                    owner_id = model.tbl_Owner.owner_id
                };

                _repository.AddRestaurant(restaurant);



                return RedirectToAction("MenuItems");
            }
            return View(model);
        }

        [Route("MenuItems")]
        public IActionResult MenuItems()
        {
            return View();
        }

        [Route("home")]
        public IActionResult home()
        {
            return View();
        }
    }
}
