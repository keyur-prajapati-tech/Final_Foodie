using System.Net.Mail;
using System.Net;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;
using DocumentFormat.OpenXml.Spreadsheet;

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

        [HttpGet("VendorApproval/Details/{id}")]
        public IActionResult Details(int id)
        {
            var vendor = _AdminRepository.GetVendorById(id);
            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }
        [HttpPost]
        public IActionResult RejectVendor(int id, string email)
        {
            var success = _AdminRepository.DeleteVendor(id);

            if (success)
            {
                string subject = "Restaurant Rejected";
                string body = "<h3>We're sorry!</h3><p>Your restaurant registration has been rejected.</p>";
                SendEmail(email, subject, body);

                TempData["message"] = "Vendor rejected and notified successfully.";
            }
            else
            {
                TempData["error"] = "Failed to delete vendor.";
            }

            return RedirectToAction("Restaurant", "AddRestaurant");
        }

        public IActionResult ChangeStatus(int resid, bool status)
        {
            bool success = _AdminRepository.ChangeResStatus(resid, status);

            if (success)
                TempData["Success"] = $"Restaurant status changed to {status} successfully";
            else
                TempData["Error"] = "Failed to update partner status";

            return RedirectToAction("Restaurant", "AddRestaurant");
        }
        public IActionResult Approve(int id , string email)
        {
            string subject = "Restaurant Approved";
            string body = "<h3>Your restaurant has been approved!</h3><p>You can now receive orders on our platform.</p>";
            SendEmail(email, subject, body);
            return ChangeStatus(id, true);
        }

        public IActionResult Reject(int id , string email)
                    {
            string subject = "Restaurant Suspended";
            string body = "<h3>We're sorry!</h3><p>Your restaurant registration has been Suspended.</p>";
            SendEmail(email, subject, body);
            return ChangeStatus(id, false);
        }
        public void SendEmail(string toEmail, string subject, string body)
        {
            var fromEmail = "amishaambaliya12203@gmail.com";
            var fromPassword = "sorgvexrsillzxmj";

            var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true
            };

            smtp.Send(message);
        }
    }
}
