using Foodie.Models.customers;
using Foodie.ViewModels;
using Foodie.Repositories;
using Microsoft.AspNetCore.Mvc;
using Foodie.Models.Restaurant;
using ClosedXML.Excel;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace Foodie.Controllers.Restaurant
{
    public class OrderHistoryController : Controller
    {
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IWebHostEnvironment _env;

        public OrderHistoryController(IRestaurantRepository restaurantRepository, IWebHostEnvironment env)
        {
            _restaurantRepository = restaurantRepository;
            _env = env;
        }

        public IActionResult OrdHistory()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId")); ; // Replace with the actual restaurant ID

            var orderHistory = _restaurantRepository.tbl_Orders_History(restaurantId);

            return View(orderHistory);
        }


        public IActionResult offers()
        {
            var offers = _restaurantRepository.GetAllOffers();
            return View(offers);
        }

        [HttpPost]
        public async Task<IActionResult> AddOffer(tbl_special_offers offer, List<IFormFile> image_path)
        {
            if (!ModelState.IsValid)
            {
                string imagePaths = "";

                if (image_path != null && image_path.Count > 0)
                {
                    foreach (var file in image_path)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(_env.WebRootPath, "Uploads", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        imagePaths += "/Uploads/" + fileName + ",";
                    }

                    imagePaths = imagePaths.TrimEnd(',');
                    offer.ImagePath = imagePaths;
                }

                _restaurantRepository.AddOffeer(offer);
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Validation failed" });
        }

        [HttpGet]
        public IActionResult GetOffer(int id)
        {
            var offer = _restaurantRepository.GetOfferById(id);

            if(offer == null)
            {
                return NotFound();
            }
            return Json(offer);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOffer(tbl_special_offers offer, List<IFormFile> image_path)
        {
            if (!ModelState.IsValid)
            {
                string imagePaths = "";

                if (image_path != null && image_path.Count > 0)
                {
                    foreach (var file in image_path)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(_env.WebRootPath, "Uploads", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        imagePaths += "/Uploads/" + fileName + ",";
                    }

                    imagePaths = imagePaths.TrimEnd(',');

                    offer.ImagePath = imagePaths;
                }

                _restaurantRepository.UpdateOffer(offer);
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Validation Failed" });
        }

        [HttpPost]
        public IActionResult deleteOffer(int id)
        {
            _restaurantRepository.DeleteOffer(id);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult SearchOffers(DateTime? validFrom, DateTime? validTo)
        {
            var offers = _restaurantRepository.GetOffersByDateRange(validFrom, validTo);
            return View("offers", offers);
        }

        [HttpGet]
        public IActionResult offers(bool? isActive, DateTime? validFrom, DateTime? validTo)
        {
            IEnumerable<tbl_special_offers> offers;

            if (validFrom.HasValue || validTo.HasValue || isActive.HasValue)
            {
                offers = _restaurantRepository.GetOffersByDateAndStatus(validFrom, validTo, isActive);
            }
            else
            {
                offers = _restaurantRepository.GetAllOffers();
            }

            return View(offers);
        }

        [HttpGet]
        public IActionResult ExportToExcel(bool? isActive, DateTime? validFrom, DateTime? validTo)
        {
            IEnumerable<tbl_special_offers> offers;

            if (validFrom.HasValue || validTo.HasValue || isActive.HasValue)
            {
                offers = _restaurantRepository.GetOffersByDateAndStatus(validFrom, validTo, isActive);
            }
            else
            {
                offers = _restaurantRepository.GetAllOffers();
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SpecialOffers");
                var currentRow = 1;

                // Header
                worksheet.Cell(currentRow, 1).Value = "Offer Title";
                worksheet.Cell(currentRow, 2).Value = "Description";
                worksheet.Cell(currentRow, 3).Value = "Discount (%)";
                worksheet.Cell(currentRow, 4).Value = "Valid From";
                worksheet.Cell(currentRow, 5).Value = "Valid To";
                worksheet.Cell(currentRow, 6).Value = "Status";

                // Data
                foreach (var offer in offers)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = offer.offer_title;
                    worksheet.Cell(currentRow, 2).Value = offer.offer_desc;
                    worksheet.Cell(currentRow, 3).Value = offer.percentage_disc;
                    worksheet.Cell(currentRow, 4).Value = offer.validFrom.ToShortDateString();
                    worksheet.Cell(currentRow, 5).Value = offer.validTo.ToShortDateString();
                    worksheet.Cell(currentRow, 6).Value = offer.is_Active ? "Active" : "Inactive";
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SpecialOffers.xlsx");
                }
            }
        }

        [HttpGet]
        public IActionResult ExportToPDF(bool? isActive, DateTime? validFrom, DateTime? validTo)
        {
            IEnumerable<tbl_special_offers> offers;

            if (validFrom.HasValue || validTo.HasValue || isActive.HasValue)
            {
                offers = _restaurantRepository.GetOffersByDateAndStatus(validFrom, validTo, isActive);
            }
            else
            {
                offers = _restaurantRepository.GetAllOffers();
            }

            using (MemoryStream stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                document.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Paragraph title = new Paragraph("Special Offers Report", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                title.SpacingAfter = 20;
                document.Add(title);

                // Table
                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2, 3, 1, 1.5f, 1.5f, 1 });

                // Header
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                table.AddCell(new Phrase("Offer Title", headerFont));
                table.AddCell(new Phrase("Description", headerFont));
                table.AddCell(new Phrase("Discount", headerFont));
                table.AddCell(new Phrase("Valid From", headerFont));
                table.AddCell(new Phrase("Valid To", headerFont));
                table.AddCell(new Phrase("Status", headerFont));

                // Data
                var dataFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                foreach (var offer in offers)
                {
                    table.AddCell(new Phrase(offer.offer_title, dataFont));
                    table.AddCell(new Phrase(offer.offer_desc, dataFont));
                    table.AddCell(new Phrase(offer.percentage_disc.ToString() + "%", dataFont));
                    table.AddCell(new Phrase(offer.validFrom.ToShortDateString(), dataFont));
                    table.AddCell(new Phrase(offer.validTo.ToShortDateString(), dataFont));
                    table.AddCell(new Phrase(offer.is_Active ? "Active" : "Inactive", dataFont));
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "SpecialOffers.pdf");
            }
        }


        public IActionResult reports()
        {
            return View();
        }

        public IActionResult payouts()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId")); // Or wherever you're storing it
            var viewModel = _restaurantRepository.GetBankDetailsByRestaurantId(restaurantId);

            if (viewModel == null || viewModel.BankDetails == null)
            {
                // Optional: handle empty state (no bank details found)
                ViewBag.Message = "No bank details found.";
            }

            return View(viewModel);
        }
        [HttpGet]
        public IActionResult GetBadOrderStats()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            var stats = _restaurantRepository.GetWeeklyOrderStatsAsync(restaurantId);
            return Json(stats);
        }
        [HttpGet]
        public  IActionResult GetWeeklyRatings()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            var data =  _restaurantRepository.GetWeeklyCustomerRatings(restaurantId);
            return Json(data);
        }
        public IActionResult complaint()
            {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            var vendor = _restaurantRepository.GetComplaintsByRestaurantId(restaurantId);
            return View(vendor);
        }

        [HttpGet]
        public IActionResult GetPerformanceMetrics()
        {
            var result = _restaurantRepository.GetPerformanceMetrics();
            return Json(result);
        }

        [HttpPost]
        public IActionResult EditComplaint(tbl_cust_vendor_complaints tbl)
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            tbl.restaurant_id = restaurantId;
            _restaurantRepository.updateVencom(tbl);
            return RedirectToAction("complaint");
        }
        public IActionResult Reviews()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId"));
            var ratings = _restaurantRepository.GetAllRatings(restaurantId);
            return View(ratings);
        }

        public IActionResult OutletInfo()
        {
            var restaurantId = Convert.ToInt32(HttpContext.Session.GetString("UserId")); // Replace with the actual restaurant ID
            var model = _restaurantRepository.GetOutletInfo(restaurantId);
            return View(model);
        }
        [HttpPost]
        public IActionResult EditOutletInfo(OutletInfo model)
        {
            if (model.NewRestaurantImg != null)
            {
                using var ms = new MemoryStream();
                model.NewRestaurantImg.CopyTo(ms);
                model.restaurant_img = ms.ToArray();
            }

            if (model.NewRestaurantMenuImg != null)
            {
                using var ms = new MemoryStream();
                model.NewRestaurantMenuImg.CopyTo(ms);
                model.restaurant_menu_img = ms.ToArray();
            }

            _restaurantRepository.UpdateOutletInfo(model);
            TempData["Success"] = "Outlet information updated successfully.";
            return RedirectToAction("OutletInfo", new { id = model.restaurant_id });
        }

    }
}
