//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using bus_transport_mgt_sys.Models;
//using Microsoft.EntityFrameworkCore;

//namespace bus_transport_mgt_sys.Pages.Dashboards.Payments
//{
//    public class CreateModel : PageModel
//    {
//        private readonly bus_transport_mgt_sys.Models.bus_transportationDBContext _context;

//        public CreateModel(bus_transport_mgt_sys.Models.bus_transportationDBContext context)
//        {
//            _context = context;
//        }

//        [BindProperty]
//        public string PhoneNumber { get; set; }

//        [BindProperty]
//        public string ReferenceCode { get; set; }

//        [BindProperty]
//        public decimal Amount { get; set; }

//        [BindProperty]
//        public int RouteId { get; set; }

//        [BindProperty]
//        public int ScheduleId { get; set; }

//        public string PaymentStatusMessage { get; set; }

//        public async Task<IActionResult> OnPostAsync()
//        {
//            // Validate phone number format
//            if (!(PhoneNumber.StartsWith("078") || PhoneNumber.StartsWith("073")))
//            {
//                return new JsonResult(new { Status = "FAILURE", Message = "Invalid phone number!" });
//            }

//            // Simulate payment success or failure
//            bool isPaymentSuccessful = ReferenceCode == "123456"; // Mock success with specific reference code

//            if (!isPaymentSuccessful)
//            {
//                return new JsonResult(new { Status = "FAILURE", Message = "Payment failed! Please retry." });
//            }

//            // Step 1: Retrieve schedule information
//            var schedule = await _context.Schedules.FirstOrDefaultAsync(s => s.ScheduleId == ScheduleId);

//            if (schedule == null)
//            {
//                return new JsonResult(new { Status = "FAILURE", Message = "Invalid schedule ID!" });
//            }

//            // Step 2: Generate a Ticket
//            var ticket = new Ticket
//            {
//                BusId = schedule.BusId,  // Assuming the BusId is available from the schedule
//                UserId = 1,  // This should be replaced with the actual user ID
//                ScheduleId = schedule.ScheduleID,
//            };
//            _context.Tickets.Add(ticket);
//            await _context.SaveChangesAsync(); // Save ticket to generate TicketId

//            // Step 3: Store Payment Details
//            var payment = new Payment
//            {
//                TicketId = ticket.TicketId, // Link payment to the generated ticket
//                Amount = Amount > 0 ? Amount : schedule.Amount, // Use the provided amount or the schedule amount
//                PaymentMethod = "Mobile Money",  // Assuming Mobile Money as the payment method
//                PaymentDate = DateTime.Now
//            };
//            _context.Payments.Add(payment);
//            await _context.SaveChangesAsync();

//            // Step 4: Respond with Ticket Details
//            var response = new
//            {
//                Status = "SUCCESS",
//                Message = "Payment successful! Your ticket has been confirmed.",
//                TicketDetails = new
//                {
//                    TicketId = ticket.TicketId,
//                    RouteId = schedule.Bus.RouteId,  // Assuming RouteId is related to Schedule
//                    ScheduleId = schedule.ScheduleId,
//                    BusId = schedule.BusId,
//                    IssueDate = DateTime.Now,
//                    SeatNumber = "A1"  // For simplicity, assign a fixed seat. Modify as needed.
//                }
//            };

//            return new JsonResult(response);
//        }
//    }
//}
