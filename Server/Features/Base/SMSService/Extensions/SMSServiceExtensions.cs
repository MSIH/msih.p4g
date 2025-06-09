using Microsoft.Extensions.DependencyInjection;
using msih.p4g.Server.Features.Base.SMSService.Interfaces;
using msih.p4g.Server.Features.Base.SMSService.Services;

namespace msih.p4g.Server.Features.Base.SMSService.Extensions
{
    public static class SMSServiceExtensions
    {
        public static IServiceCollection AddTwilioSMSService(this IServiceCollection services)
        {
            // Register HttpClient for Twilio service
            services.AddHttpClient<TwilioSMSService>();
            
            // Register Twilio SMS service as the implementation of ISMSService
            services.AddScoped<ISMSService, TwilioSMSService>();
            
            return services;
        }
        
        public static IServiceCollection AddDevelopmentSMSService(this IServiceCollection services)
        {
            // Register development SMS service
            services.AddScoped<ISMSService, DevelopmentSMSService>();
            
            return services;
        }
        
        public static IServiceCollection AddSMSService(this IServiceCollection services, IWebHostEnvironment environment)
        {
            // Choose the appropriate SMS service based on the environment
            if (environment.IsDevelopment())
            {
                services.AddDevelopmentSMSService();
            }
            else
            {
                services.AddTwilioSMSService();
            }
            
            return services;
        }
    }
}