using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace vtb.Messaging.Configuration
{
    public class BusHostedService : IHostedService
    {
        private readonly ILogger<BusHostedService> _logger;
        private readonly BusInstallator _busInstallator;

        public BusHostedService(ILogger<BusHostedService> logger, BusInstallator busInstallator)
        {
            _logger = logger;
            _busInstallator = busInstallator;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _busInstallator.Install();
            _logger.LogInformation("Bus installed.");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}