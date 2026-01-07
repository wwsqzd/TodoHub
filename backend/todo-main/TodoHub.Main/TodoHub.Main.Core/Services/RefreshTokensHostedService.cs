
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class RefreshTokensHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public RefreshTokensHostedService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Loop until the service is stopped
            while (!stoppingToken.IsCancellationRequested)
            {
                // Create a new service scope to get scoped services
                using var scope = _scopeFactory.CreateScope();
                var tokensService = scope.ServiceProvider.GetRequiredService<IRefreshTokensCleanerService>();

                // delete all old refresh tokens
                await tokensService.CleanAllRefreshTokens(stoppingToken);

                // Wait 12 Hours
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }
    }
}
