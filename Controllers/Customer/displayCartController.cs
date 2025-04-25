using Foodie.Models.customers;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Customer
{
    public class displayCartController : Controller
    {
        private readonly IcustomerRepository _repository;

        public displayCartController(IcustomerRepository repository)
        {
            _repository = repository;
        }

        //[HttpGet]
        //public IActionResult MenuItemDetails(int menuId)
        //{
        //    var item = _repository.GetMenuItem(menuId);
        //    if(item == null)
        //    {
        //        return NotFound();
        //    }
        //    return PartialView("_MenuItemModalPartial",item);
        //}

       public IActionResult AddToCartIteminfo(int menuid,int quantity,decimal price)
       {
            int customerId = int.Parse(HttpContext.Session.GetString("customer_id"));

            var cart = _repository.GetOrCreateCart(customerId);

            _repository.AddToCart(new Models.customers.tbl_cart_item
            {
                menu_id = menuid,
                cart_id = cart.cart_id,
                quantity = quantity,
                price = price
            });

            return Json(new { success = true, message = "Item Add To Cart Successfully...!" });
       }

        //public IActionResult MyCart()
        //{
        //    int customerId = int.Parse(HttpContext.Session.GetString("customer_id"));
        //    var cartItems = _repository.GetCartItems(customerId);
        //    if (cartItems != null && cartItems.Count > 0)
        //    {
        //        return View(cartItems);
        //    }
        //    else
        //    {
        //        return View("EmptyCart");
        //    }
        //}

        //public IActionResult Checkout()
        //{
        //    int customerId = int.Parse(HttpContext.Session.GetString("CustomerId"));
        //    _repository.Placeorder(customerId);
        //    return RedirectToAction("OrderSuccess");
        //}
    }
}
