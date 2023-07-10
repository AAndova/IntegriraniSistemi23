using EShop.Domain.DomainModels;
using EShop.Domain.DTO;
using EShop.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using OfficeOpenXml;

namespace EShop.Web.Controllers
{
    public class TicketExportController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketExportController> _logger;

        public TicketExportController(
            ILogger<TicketExportController> logger,
            ITicketService ticketService)
        {
            _logger = logger;
            _ticketService = ticketService;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult ExportAllTickets()
        {
            var tickets = _ticketService.GetAllTickets();
            byte[] fileContents = GenerateExcelFile(tickets);
            string fileName = "AllTickets.xlsx";

            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult ExportTicketsByGenre(string genre)
        {
            var tickets = _ticketService.GetTicketsByGenre(genre);
            byte[] fileContents = GenerateExcelFile(tickets);
            string fileName = $"{genre}Tickets.xlsx";

            return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private byte[] GenerateExcelFile(List<Ticket> tickets)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Tickets");

                // Set the column headers
                worksheet.Cells[1, 1].Value = "Ticket Name";
                worksheet.Cells[1, 2].Value = "Ticket Price";
                worksheet.Cells[1, 3].Value = "Genre";

                // Populate the data rows
                for (int i = 0; i < tickets.Count; i++)
                {
                    var ticket = tickets[i];
                    worksheet.Cells[i + 2, 1].Value = ticket.TicketName;
                    worksheet.Cells[i + 2, 2].Value = ticket.TicketPrice;
                    worksheet.Cells[i + 2, 3].Value = ticket.Genre;
                }

                // Auto fit the columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }
    }
}