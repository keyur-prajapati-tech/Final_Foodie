using DocumentFormat.OpenXml.Bibliography;
using Foodie.Repositories;
using Foodie.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
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
        public IActionResult GetWeeklySales(int year, int month)
        {
            var sales = _AdminRepository.GetWeeklySales(year, month);
            return Json(sales);
        }
        [HttpGet]
        public FileResult DownloadReport(int year, int month)
        {
            var sales = _AdminRepository.GetWeeklySales(year, month);

            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 25, 25, 30, 30); // Page size and margins
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // Fonts
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16, BaseColor.BLACK);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

                // Add Title
                Paragraph title = new Paragraph($"📊 Sales Report for {System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)} {year}", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                doc.Add(title);

                // Create table with 3 columns
                PdfPTable table = new PdfPTable(2)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10f,
                    SpacingAfter = 10f
                };
                table.SetWidths(new float[] { 1f, 2f });

                // Table Header with Background Color
                PdfPCell cell1 = new PdfPCell(new Phrase("Week Number", headerFont))
                {
                    BackgroundColor = new BaseColor(0, 120, 215), // Blue header
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                };
                PdfPCell cell2 = new PdfPCell(new Phrase("Total Amount", headerFont))
                {
                    BackgroundColor = new BaseColor(0, 120, 215),
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                };
                table.AddCell(cell1);
                table.AddCell(cell2);

                // Add Rows
                foreach (var item in sales)
                {
                    PdfPCell weekCell = new PdfPCell(new Phrase(item.WeekNumber.ToString(), cellFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    PdfPCell amountCell = new PdfPCell(new Phrase(item.TotalAmount.ToString("C"), cellFont))
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 5
                    };
                    table.AddCell(weekCell);
                    table.AddCell(amountCell);
                }

                doc.Add(table);

                // Footer Note
                Paragraph footer = new Paragraph($"Generated on {DateTime.Now:dd MMM yyyy}", FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 20
                };
                doc.Add(footer);

                doc.Close();

                return File(ms.ToArray(), "application/pdf", $"SalesReport_{year}_{month}.pdf");
            }
        }

        [HttpGet]
        public IActionResult GetYearlySales(int year)
        {
            var sales = _AdminRepository.GetYearlySales(year);
            return Json(sales);
        }

        [HttpGet]
        public FileResult DownloadYearlyReport(int year)
        {
            var sales = _AdminRepository.GetYearlySales(year);

            using (var ms = new MemoryStream())
            {
                Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
                var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                doc.Add(new Paragraph($"📈 Yearly Sales Report for {year}", titleFont) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 20 });

                PdfPTable table = new PdfPTable(2) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 1f, 2f });

                table.AddCell(new PdfPCell(new Phrase("Month", headerFont)) { BackgroundColor = new BaseColor(0, 120, 215), Padding = 5 });
                table.AddCell(new PdfPCell(new Phrase("Total Amount", headerFont)) { BackgroundColor = new BaseColor(0, 120, 215), Padding = 5 });

                foreach (var item in sales)
                {
                    string monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.SalesMonth);
                    table.AddCell(new PdfPCell(new Phrase(monthName, cellFont)) { Padding = 5 });
                    table.AddCell(new PdfPCell(new Phrase(item.TotalSales.ToString("C"), cellFont)) { Padding = 5, HorizontalAlignment = Element.ALIGN_RIGHT });
                }

                doc.Add(table);
                doc.Add(new Paragraph($"Generated on {DateTime.Now:dd MMM yyyy}", FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9)) { Alignment = Element.ALIGN_RIGHT });

                doc.Close();
                return File(ms.ToArray(), "application/pdf", $"YearlySalesReport_{year}.pdf");
            }
        }
       
    }

}
