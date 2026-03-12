using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArenaBackend.Services
{
    public class BillingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BillingBackgroundService> _logger;
        // Run daily
        private static readonly TimeSpan _period = TimeSpan.FromDays(1);

        public BillingBackgroundService(IServiceProvider serviceProvider, ILogger<BillingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Billing Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();

                        _logger.LogInformation("Running GenerateMonthlyChargesAsync...");
                        await billingService.GenerateMonthlyChargesAsync();

                        _logger.LogInformation("Running CheckOverduePaymentsAsync...");
                        await billingService.CheckOverduePaymentsAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing billing background tasks.");
                }

                _logger.LogInformation("Billing Background Service sleeping...");
                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("Billing Background Service is stopping.");
        }
    }
}
