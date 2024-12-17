using System.Threading.Tasks;

namespace bus_transport_mgt_sys.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
