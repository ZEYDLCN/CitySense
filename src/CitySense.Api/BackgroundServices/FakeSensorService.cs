using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CitySense.Domain.Interfaces;

namespace CitySense.Api.Services
{
    public class FakeSensorService : IHostedService, IDisposable
    {
        private readonly ILogger<FakeSensorService> _logger;
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;

        public FakeSensorService(ILogger<FakeSensorService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fake Sensor Hosted Service is starting.");

            using (var scope = _scopeFactory.CreateScope())
            {
                var orchestrator = scope.ServiceProvider.GetRequiredService<IFakeDataOrchestrator>();
                try
                {
                    await orchestrator.InitializeSensorsIfNeededAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during initial sensor initialization.");
                }
            }

            _timer = new Timer(DoWork, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("Fake Sensor Hosted Service triggered. Generating data point...");
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var orchestrator = scope.ServiceProvider.GetRequiredService<IFakeDataOrchestrator>();
                    orchestrator.GenerateAndSaveSensorDataPointAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in FakeSensorService.DoWork while generating data point.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fake Sensor Hosted Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
