using bus_transport_mgt_sys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace bus_transport_mgt_sys.Pages.Dashboards.Tickets
{
    public class ConfirmationModel : PageModel
    {
        private readonly bus_transportationDBContext _context;

        public ConfirmationModel(bus_transportationDBContext context)
        {
            _context = context;
        }

        public TicketViewModel Ticket { get; set; }

        public Ticket Tickets { get; set; }
        public async Task<IActionResult> OnGetAsync(int TicketId)
        {
            // Retrieve ticket details with comprehensive include
            var ticket = await _context.Tickets
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Bus)
                        .ThenInclude(b => b.Route)
                .Include(t => t.Payments)
                .Where(t => t.TicketId == TicketId)
                .Select(t => new TicketViewModel
                {
                    TicketId = t.TicketId,
                    RouteName = t.Schedule.Bus.Route.RouteName,
                    Departure = t.Schedule.Bus.Route.Departure,
                    Destination = t.Schedule.Bus.Route.Destination,
                    BusPlateNo = t.Schedule.Bus.PlateNo,
                    BusModel = t.Schedule.Bus.Model,
                    DepartureTime = t.Schedule.DepartureTime,
                    ArrivalTime = t.Schedule.ArrivalTime,
                    Amount = t.Schedule.Amount,
                    PaymentDate = t.Payments.FirstOrDefault().PaymentDate,
                    PaymentMethod = t.Payments.FirstOrDefault().PaymentMethod
                })
                .FirstOrDefaultAsync();

            // Handle case when ticket is not found
            if (ticket == null)
            {
                return NotFound($"Ticket with ID {TicketId} not found.");
            }

            Ticket = ticket;
            return Page();
        }

        public IActionResult OnGetDownloadPdf(int TicketId)
        {
            // Fetch the ticket with related entities
            var ticket = _context.Tickets
                .Include(t => t.Schedule)
                    .ThenInclude(s => s.Bus)
                        .ThenInclude(b => b.Route)
                .Include(t => t.Payments)
                .FirstOrDefault(t => t.TicketId == TicketId);

            // Check if ticket exists
            if (ticket == null)
            {
                return NotFound($"Ticket with ID {TicketId} not found.");
            }

            try
            {
                // Generate the PDF using QuestPDF
                byte[] pdfBytes = Document.Create(document =>
                {
                    document.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Helvetica"));

                        // Header Title
                        page.Header().Text("Ticket Confirmation")
                            .SemiBold()
                            .FontSize(20)
                            .AlignCenter();

                        // Content section
                        page.Content().Column(column =>
                        {
                            column.Spacing(10);

                            column.Item().Text($"Ticket ID: {ticket.TicketId}");
                            column.Item().Text($"Route: {ticket.Schedule.Bus.Route.RouteName}");
                            column.Item().Text($"Departure: {ticket.Schedule.DepartureTime:g}");
                            column.Item().Text($"Destination: {ticket.Schedule.Bus.Route.Destination}");
                            column.Item().Text($"Bus Plate No: {ticket.Schedule.Bus.PlateNo}");
                            column.Item().Text($"Bus Model: {ticket.Schedule.Bus.Model}");
                            column.Item().Text($"Departure Time: {ticket.Schedule.DepartureTime:g}");
                            column.Item().Text($"Arrival Time: {ticket.Schedule.ArrivalTime:g}");
                            column.Item().Text($"Amount Paid: ${ticket.Schedule.Amount}");

                            var payment = ticket.Payments.FirstOrDefault();
                            if (payment != null)
                            {
                                column.Item().Text($"Payment Date: {payment.PaymentDate:g}");
                                column.Item().Text($"Payment Method: {payment.PaymentMethod}");
                            }
                            else
                            {
                                column.Item().Text("Payment Date: N/A");
                                column.Item().Text("Payment Method: N/A");
                            }
                        });

                        // Optional footer
                        page.Footer()
                            .AlignRight()
                            .Text(text =>
                            {
                                text.Span("Generated on: ").SemiBold();
                                text.Span(DateTime.Now.ToString("g"));
                            });
                    });
                }).GeneratePdf();

                // Return the PDF as a file
                return File(pdfBytes, "application/pdf", $"Ticket_{TicketId}_Confirmation.pdf");
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.Error.WriteLine($"Error generating PDF: {ex.Message}");

                // Return an error response
                return StatusCode(500, "An error occurred while generating the PDF.");
            }
        }




        public class TicketViewModel
        {
            public int TicketId { get; set; }
            public string RouteName { get; set; }
            public string Departure { get; set; }
            public string Destination { get; set; }
            public string BusPlateNo { get; set; }
            public string BusModel { get; set; }
            public DateTime DepartureTime { get; set; }
            public DateTime ArrivalTime { get; set; }
            public decimal Amount { get; set; }
            public DateTime PaymentDate { get; set; }
            public string PaymentMethod { get; set; }
        }
    }
}