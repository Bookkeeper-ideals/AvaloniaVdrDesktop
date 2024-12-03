using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace VdrDesktop
{
    public class BackgroundFileSyncService : IHostedService
    {
        private Timer? _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Background File Sync Service Starting...");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            // Simulate background work
            Console.WriteLine($"Syncing files at {DateTime.Now}");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Background File Sync Service Stopping...");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}
