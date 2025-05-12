using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Foodie.Controllers
{
    public class VendoreApprovalController : Controller
    {
        private readonly IAdminRepository _AdminRepository;

        public VendoreApprovalController(IAdminRepository adminRepository)
        {
            _AdminRepository = adminRepository;
        }
        public IActionResult VendorApproval()
        {
            var model = _AdminRepository.GetVendorsForApproval();
            return View(model);
        }

        [HttpPost]
        public IActionResult ApproveVendor(int id)
        {
            _AdminRepository.UpdateApprovalStatus(id, true);
            return RedirectToAction("VendorApproval");
        }

        [HttpPost]
        public IActionResult RejectVendor(int id)
        {
            var success = _AdminRepository.DeleteVendor(id);
            if (success)
                TempData["message"] = "Vendor rejected and deleted successfully.";
            else
                TempData["error"] = "Failed to delete vendor.";

            return RedirectToAction("VendorApproval");
        }
    }
}
