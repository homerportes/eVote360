
using eVote360.Application.Interfaces;
using eVote360.Domain.Settings;
using eVote360.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvestmentApp.Infrastructure.Persistence
{
    public static class ServicesRegistration
    {
        public static void AddSharedLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Configuration
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            #endregion

            #region Services IOC
            services.AddScoped<IEmailService, EmailService>();

            // OCR service
            services.AddScoped<IOcrService, TesseractOcrService>();
            #endregion
        }
    }
}
