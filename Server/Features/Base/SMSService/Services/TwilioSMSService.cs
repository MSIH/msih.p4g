using msih.p4g.Server.Features.Base.SMSService.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.SMSService.Services
{
    public class TwilioSMSService : ISMSService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromPhone;
        private readonly ILogger<TwilioSMSService> _logger;
        private readonly HttpClient _httpClient;

        public TwilioSMSService(IConfiguration configuration, ILogger<TwilioSMSService> logger, HttpClient httpClient)
        {
            _accountSid = configuration["Twilio:AccountSid"] ?? throw new Exception("Twilio AccountSid not configured");
            _authToken = configuration["Twilio:AuthToken"] ?? throw new Exception("Twilio AuthToken not configured");
            _fromPhone = configuration["Twilio:FromPhone"] ?? throw new Exception("Twilio FromPhone not configured");
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task SendSMSAsync(string to, string? from, string message)
        {
            try
            {
                // Use the from parameter if provided, otherwise fall back to configuration
                var senderPhone = !string.IsNullOrWhiteSpace(from) ? from : _fromPhone;

                // Twilio API endpoint for sending SMS
                var apiUrl = $"https://api.twilio.com/2010-04-01/Accounts/{_accountSid}/Messages.json";

                // Create the authorization header using basic auth with AccountSid and AuthToken
                var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_accountSid}:{_authToken}"));
                
                // Create request content
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("To", to),
                    new KeyValuePair<string, string>("From", senderPhone),
                    new KeyValuePair<string, string>("Body", message)
                });

                // Create and send the request
                using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authToken);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var sid = responseData.GetProperty("sid").GetString();
                    _logger.LogInformation($"SMS sent successfully with SID: {sid}");
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to send SMS: {response.StatusCode} - {errorBody}");
                    throw new Exception($"Failed to send SMS: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS using Twilio");
                throw;
            }
        }
    }
}