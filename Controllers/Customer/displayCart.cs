using Foodie.Models.customers;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers.Customer
{
    public class displayCart : Controller
    {
        private readonly IcustomerRepository _repository;

        public displayCart(IcustomerRepository repository)
        {
            _repository = repository;
        }

        public IActionResult AddToCartIteminfo(int menuId, int quantity, decimal price)
        {
            string customerIdString = HttpContext.Session.GetString("customer_id");

            if (string.IsNullOrEmpty(customerIdString))
            {
                return Json(new { success = false, message = "User not logged in." });
            }

            int customerId = int.Parse(customerIdString);

            var menuItem = _repository.GetMenuItem(menuId);

            if (menuItem == null)
            {
                return Json(new { success = false, message = "Menu item not found." });
            }

            var cart = _repository.GetOrCreatecart(customerId);
            if (cart == null)
            {
                return Json(new { success = false, message = "Cart not found." });
            }

            try
            {
                _repository.AddToCart(new tbl_cart_item
                {
                    customer_id = customerId,
                    cart_id = cart.cart_id,
                    menu_id = menuId,
                    quantity = quantity,
                    price = price
                });

                return Json(new { success = true, message = "Item added to cart successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding item to cart: " + ex.Message });
            }
        }

        public IActionResult MyCart()
        {
            int customerId = int.Parse(HttpContext.Session.GetString("customer_id"));
            var cartItems = _repository.GetCartItems(customerId);
            if (cartItems != null && cartItems.Count > 0)
            {
                return View(cartItems);
            }
            else
            {
                return View("EmptyCart");
            }
        }

        public IActionResult Checkout()
        {
            int customerId = int.Parse(HttpContext.Session.GetString("CustomerId"));
            _repository.Placeorder(customerId);
            return RedirectToAction("OrderSuccess");
        }
    }
}
