using Foodie.Repositories;
using Foodie.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Foodie.Controllers
{
    
    public class OrdersController : Controller
    {
        private readonly IAdminRepository _AdminRepository;

        public OrdersController(IAdminRepository adminRepository)
        {
            _AdminRepository = adminRepository;
        }

        public IActionResult oders()
        {
           

            return View();
        }

        [HttpGet]
        public IActionResult oders1(string status)
        {
            var orders = _AdminRepository.GetAllOrders(status);

            return Json(orders);
        }
        public IActionResult DashBoard()
        {
            var vm = new DashBoardViewModel
            {
                MonthlySales = _AdminRepository.GetMonthlySales(),
                MonthlyCustomers = _AdminRepository.GetMonthlyCustomerCount(),
                MonthlyRestaurants = _AdminRepository.GetMonthlyRestaurantCount(),

                CancelledOrders = _AdminRepository.GetCancelledOrders(),
                PendingOrders = _AdminRepository.GetPendingOrders(),
                AcceptedOrders = _AdminRepository.GetAcceptedOrders(),
                DeliverdOrders = _AdminRepository.GetDeliveredOrders(),

                ActiveRestaurants = _AdminRepository.GetActiveRestaurants(),
                InactiveRestaurants = _AdminRepository.GetInactiveRestaurants(),
                OpenRestaurants = _AdminRepository.GetOpenRestaurants(),
                ClosedRestaurants = _AdminRepository.GetClosedRestaurants(),

                //MonthlySalesChart = _AdminRepository.GetMonthlySalesData(),
               // MonthlyLineChart = _adminRepo.GetLineChartData()
            };
            ViewBag.MonthlySales = vm.MonthlySales;
            return View(vm);
        }
        [HttpGet]
        public IActionResult GetMonthlySalesData()
        {
            var salesData = _AdminRepository.GetMonthlySalesData(); // List<int>

            var result = new
            {
                labels = salesData.Month,
                datasets = new[]
                {
                    new
                    {
                     
                         label = "Monthly Sales",
                                data =  salesData.MonthlySalesdata,
                                backgroundColor = "#000957",
                                borderColor =  "#8697C4",
                                borderWidth = 2

                    }
                }
            };
            return Json(result);
        }
        public IActionResult GetYearlySalesData()
        {
            var salesData = _AdminRepository.GetYearlyChartData(); // List<int>

            var result = new
            {
                labels = salesData.Year,
                datasets = new[]
                {
                    new
                    {

                         label = "Monthly Sales",
                                data =  salesData.YearlySalesdata,
                                backgroundColor = "#000957",
                                borderColor =  "#8697C4",
                                borderWidth = 2

                    }
                }
            };
            return Json(result);
        }
    }

}
