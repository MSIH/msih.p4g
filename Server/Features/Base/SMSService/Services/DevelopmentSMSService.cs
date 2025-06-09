using msih.p4g.Server.Features.Base.SMSService.Interfaces;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.SMSService.Services
{
    public class DevelopmentSMSService : ISMSService
    {
        private readonly ILogger<DevelopmentSMSService> _logger;

        public DevelopmentSMSService(ILogger<DevelopmentSMSService> logger)
        {
            _logger = logger;
        }

        public Task SendSMSAsync(string to, string? from, string message)
        {
            var senderPhone = !string.IsNullOrWhiteSpace(from) ? from : "DevelopmentPhone";
            
            _logger.LogInformation("======== DEVELOPMENT SMS ========");
            _logger.LogInformation($"From: {senderPhone}");
            _logger.LogInformation($"To: {to}");
            _logger.LogInformation($"Message: {message}");
            _logger.LogInformation("=================================");
            
            return Task.CompletedTask;
        }
    }
}