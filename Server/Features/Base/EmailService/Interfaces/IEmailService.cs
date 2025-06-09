using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.EmailService.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string from, string subject, string htmlContent);
        
        /// <summary>
        /// Validates if the provided email address is in a valid format.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email is valid, false otherwise.</returns>
        bool IsValidEmail(string email);
    }
}