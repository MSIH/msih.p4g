using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.SMSService.Interfaces
{
    public interface ISMSService
    {
        /// <summary>
        /// Sends an SMS message
        /// </summary>
        /// <param name="to">The recipient phone number (in E.164 format, e.g., +12345678900)</param>
        /// <param name="from">The sender phone number (if null, will use configured default)</param>
        /// <param name="message">The message content</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SendSMSAsync(string to, string? from, string message);
    }
}