using Foodie.Models.Restaurant;
using Foodie.Repositories;
using Foodie.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using System.IO;


namespace Foodie.Controllers.Admin
{
    public class ManageOrderController : Controller
    {
        private readonly IAdminRepository _AdminRepository;

        public ManageOrderController(IAdminRepository adminRepository)
        {
            _AdminRepository = adminRepository;
        }
        public IActionResult OrderReport(DateTime? fromDate, DateTime? toDate, string orderStatus)
        {
            var orders = _AdminRepository.GetFilteredOrders(fromDate, toDate, orderStatus);

            var model = new order
            {
                StartDate = fromDate,
                EndDate = toDate,
                OrderStatus = orderStatus,
                Reports = orders
            };

            return View(model);
        }

        public ActionResult ExportToPDF(DateTime? fromDate, DateTime? toDate, string orderStatus)
        {
            var orders = _AdminRepository.GetFilteredOrders(fromDate, toDate, orderStatus);

            using (MemoryStream ms = new MemoryStream())
            {
                Document doc = new Document();
                PdfWriter.GetInstance(doc, ms);
                doc.Open();

                PdfPTable table = new PdfPTable(4);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 1.5f, 2f, 2f, 2f });

                // Header row
                var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                table.AddCell(new PdfPCell(new Phrase("Order ID", boldFont)));
                table.AddCell(new PdfPCell(new Phrase("Date", boldFont)));
                table.AddCell(new PdfPCell(new Phrase("Amount", boldFont)));
                table.AddCell(new PdfPCell(new Phrase("Status", boldFont)));

                decimal total = 0;

                foreach (var order in orders)
                {
                    table.AddCell(order.OrderId.ToString());
                    table.AddCell(order.OrderDate.ToShortDateString());
                    table.AddCell(order.TotalAmount.ToString("C"));
                    table.AddCell(order.order_status);

                    total += order.TotalAmount;
                }

                // Only add grand total if status is filtered
                if (!string.IsNullOrEmpty(orderStatus))
                {
                    var grandTotalCell = new PdfPCell(new Phrase("Grand Total", boldFont))
                    {
                        Colspan = 2,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    };
                    table.AddCell(grandTotalCell);

                    table.AddCell(new PdfPCell(new Phrase(total.ToString("C"), boldFont)));
                    table.AddCell(""); // empty status cell
                }

                doc.Add(table);
                doc.Close();

                return File(ms.ToArray(), "application/pdf", "OrderReport.pdf");
            }
        }



        [HttpGet]
        public IActionResult ExportToExcel(DateTime? fromDate, DateTime? toDate, string orderStatus)
        {
            var orders = _AdminRepository.GetFilteredOrders(fromDate, toDate, orderStatus);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Order Report");

                // Add headers
                worksheet.Cell(1, 1).Value = "Order ID";
                worksheet.Cell(1, 2).Value = "Order Date";
                worksheet.Cell(1, 3).Value = "Amount";
                worksheet.Cell(1, 4).Value = "Status";

                // Fill data
                int row = 2;
                foreach (var order in orders)
                {
                    worksheet.Cell(row, 1).Value = order.OrderId;
                    worksheet.Cell(row, 2).Value = order.OrderDate.ToString("yyyy-MM-dd");
                    worksheet.Cell(row, 3).Value = order.TotalAmount;
                    worksheet.Cell(row, 4).Value = order.order_status;
                    row++;
                }

                // Auto-adjust columns
                worksheet.Columns().AdjustToContents();

                // Add total row
                worksheet.Cell(row, 2).Value = "Total";
                worksheet.Cell(row, 3).FormulaA1 = $"SUM(C2:C{row - 1})";
                worksheet.Cell(row, 3).Style.Font.Bold = true;

                // Save to stream
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = $"OrderReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }



    }
}
