using System.Net.Mail;
using System.Net;
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
        public IActionResult ApproveVendor(int id, string email)
        {
            _AdminRepository.UpdateApprovalStatus(id, true);

            string subject = "Restaurant Approved";
            string body = "<h3>Your restaurant has been approved!</h3><p>You can now receive orders on our platform.</p>";
            SendEmail(email, subject, body);

            TempData["message"] = "Vendor approved and notified successfully.";
            return RedirectToAction("VendorApproval");
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

            return RedirectToAction("VendorApproval");
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
