using ClosedXML.Excel;
using Dapper;
using AutoMailer.EmailSMTP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using System.Data;
using System.Text.RegularExpressions;

namespace AutoMailer.Controllers
{
    public class UploadController : Controller
    {
        private readonly SMTPEmailservice _smtp;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public UploadController(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtp = new SMTPEmailservice();

            // ✅ Read connection string properly
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcelToSaveEmail(IFormFile excelFile,string sender =null)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                TempData["Error"] = "Please upload a valid Excel file.";
                return RedirectToAction("Index");
            }

            var emailList = new List<string>();

            try
            {
                using var stream = new MemoryStream();
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);

                var rows = worksheet.RangeUsed()?.RowsUsed();
                if (rows == null)
                {
                    TempData["Error"] = "Excel file is empty.";
                    return RedirectToAction("Index");
                }

                // ✅ Skip header row
                foreach (var row in rows.Skip(0))
                {
                    string email = row.Cell(1).GetValue<string>()?.Trim();

                    if (!string.IsNullOrWhiteSpace(email))
                        emailList.Add(email);
                }

                // ✅ Remove duplicates (case-insensitive)
                emailList = emailList
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();


               
                    string result = await _smtp.SendEmailAsyncForRanjan(emailList);
               
               

                TempData["Success"] = $"{emailList.Count} emails processed successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error processing file: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        public IActionResult Error()
        {
            return View();
        }

    }
}


