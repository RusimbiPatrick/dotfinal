using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using bus_transport_mgt_sys.Models;

namespace bus_transport_mgt_sys.Pages.Dashboards.Payments
{
    public class IndexModel : PageModel
    {
        private readonly bus_transport_mgt_sys.Models.bus_transportationDBContext _context;

        public IndexModel(bus_transport_mgt_sys.Models.bus_transportationDBContext context)
        {
            _context = context;
        }

        public IList<Payment> Payment { get;set; } = default!;
        public IList<string> PaymentMethodsDistinct { get; set; }
        public IList<string> BusIdsDistinct { get; set; }

        public async Task OnGetAsync()
        {
            Payment = await _context.Payments
                .Include(p => p.Ticket).ToListAsync();

            PaymentMethodsDistinct = Payment.Select(p => p.PaymentMethod).Distinct().OrderBy(pm => pm).ToList();
            BusIdsDistinct = Payment.Select(p => p.Ticket.BusId).Distinct().OrderBy(b => b).ToList();
        }
    }
}
