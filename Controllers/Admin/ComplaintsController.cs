using Foodie.Models;
using Foodie.Repositories;
using Foodie.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Foodie.Controllers
{
    public class ComplaintsController : Controller
    {
        private readonly IAdminRepository _AdminRepository;

        public ComplaintsController(IAdminRepository adminRepository)
        {
            _AdminRepository = adminRepository;
        }


        public IActionResult customerComplaints()
        {
            var vendor = _AdminRepository.GetAllVendorComplaints();
            //var customer = _AdminRepository.GetAllCustomerComplaints();
            var delivery = _AdminRepository.GetAllDeliveryComplaints();

            var viewModel = new complaintsViewModel
            {
                tbl_Vendor_Complaints = vendor,
                // tbl_customer_complaints = customer,
                tbl_partner_complaints = delivery,
                tbl_cust_vendor_complaints = _AdminRepository.GetcustVendorComplaints(),
                tbl_cust_partner_complaints = _AdminRepository.GetcustPartnerComplaints()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult EditComplaint(complaintsViewModel tbl)
        {
            var adminId = HttpContext.Session.GetString("admin_id");
            //var complaint = _AdminRepository.GetvendorcomByID(complaintId);
            //if (complaint == null)
            //{
            //    return NotFound();
            //}
            tbl.tbl_Vendor_Complaint.admin_id = Convert.ToInt32(adminId);
            _AdminRepository.updateVencom(tbl.tbl_Vendor_Complaint);
            // Return a success response
            return RedirectToAction("customerComplaints");
        }
        [HttpPost]
        public IActionResult EditComplaint1(complaintsViewModel tbl)
        {
            var adminId = HttpContext.Session.GetString("admin_id");
            //var complaint = _AdminRepository.GetvendorcomByID(complaintId);
            //if (complaint == null)
            //{
            //    return NotFound();
            //}
            tbl.tbl_Partner_Complaints.admin_id = Convert.ToInt32(adminId);
            _AdminRepository.updateDelcom(tbl.tbl_Partner_Complaints);
            // Return a success response
            return RedirectToAction("customerComplaints");
        }

        public IActionResult Promotion()
        {
            return View();
        }
        public IActionResult Notification()
        {
            return View();
        }

        public IActionResult Feedaback()
        {
            var vendor = _AdminRepository.GetAllVendorFeedback();
            var customer = _AdminRepository.GetAllCustomerFeedback();
            var delivery = _AdminRepository.GetAllDeliveryFeedback();

            var viewModel = new feedbackViewModel
            {
                tbl_Vendor_Feedback = vendor,
                tbl_customer_Feedback = customer,
                tbl_Delivery_Feedback = delivery
            };

            return View(viewModel);
        }

        public IActionResult AuditLogs()
        {
            return View();
        }
    }
}
