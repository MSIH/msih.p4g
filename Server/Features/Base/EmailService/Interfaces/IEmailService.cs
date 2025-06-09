using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.EmailService.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlContent);
    }
}