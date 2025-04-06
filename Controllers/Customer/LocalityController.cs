using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Customer
{
    public class LocalityController : Controller
    {
        private readonly IcustomerRepository _repository;

        public LocalityController(IcustomerRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Locality(string DisrictName)
        {
            var cities = _repository.GetCitiesByDistrictName(DisrictName);
            return View(cities);
        }
    }
}
