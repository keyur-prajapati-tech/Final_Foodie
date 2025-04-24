using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace Foodie.Controllers
{
    public class order
    {
        public int oid;
        public string o_status;
        public string r_name;
        public string p_name;
    }
    //[Route("")]
    [Route("order")]
    public class OrdersController : Controller
    {
        [Route("order")]
        public IActionResult oders()
        {

            List<order> ord = new List<order>();

            ord.Add(new order {oid = 1,o_status="zdc",r_name="ab",p_name="xyz"});
            ord.Add(new order { oid = 2, o_status = "zdc", r_name = "ab", p_name = "xyz" });

            ViewBag.details = ord;

            ViewBag.Title = "Order Details";
            return View();
        }
        [Route("")]
        [Route("dashboard")]
        public IActionResult DashBoard()
        {
            return View();
        }
    }

}
