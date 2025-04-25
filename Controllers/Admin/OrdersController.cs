using Foodie.Repositories;
using Foodie.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

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

                MonthlySalesChart = _AdminRepository.GetMonthlySalesData(),
               // MonthlyLineChart = _adminRepo.GetLineChartData()
            };
            ViewBag.MonthlySales = vm.MonthlySales;
            return View(vm);
        }
    }

}
