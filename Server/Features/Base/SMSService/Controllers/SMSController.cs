using msih.p4g.Server.Features.Base.SMSService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace msih.p4g.Server.Features.Base.SMSService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SMSController : ControllerBase
    {
        private readonly ISMSService _smsService;
        private readonly ILogger<SMSController> _logger;

        public SMSController(ISMSService smsService, ILogger<SMSController> logger)
        {
            _smsService = smsService;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendSMS([FromBody] SendSMSRequest request)
        {
            if (string.IsNullOrEmpty(request.To) || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest("Phone number and message are required");
            }

            try
            {
                await _smsService.SendSMSAsync(request.To, request.From, request.Message);
                return Ok(new { Success = true, Message = "SMS sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS");
                return StatusCode(500, new { Success = false, Message = "Failed to send SMS" });
            }
        }
    }

    public class SendSMSRequest
    {
        public string To { get; set; } = string.Empty;
        public string? From { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}