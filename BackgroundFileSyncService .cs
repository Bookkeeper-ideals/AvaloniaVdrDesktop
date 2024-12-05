using Microsoft.Extensions.Hosting;

using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using VdrDesktop.Models;

namespace VdrDesktop
{
    public class BackgroundFileSyncService(ChannelWriter<VdrEvent> outgoingChannel, ChannelReader<VdrEvent> incommingChannel) : IHostedService
    {
        private Timer? _timer;

        private System.Timers.Timer _incomingEventsTimer;

        private Channel<string> _channel = Channel.CreateUnbounded<string>();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Background File Sync Service Starting...");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

            _incomingEventsTimer = new System.Timers.Timer(500);
            _incomingEventsTimer.Elapsed += async (sender, e) => await IncomingEventsTimer_Elapsed();
            _incomingEventsTimer.AutoReset = true; // Repeat the timer event
            _incomingEventsTimer.Enabled = true; // Start the timer

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            // Simulate background work
            Console.WriteLine($"Syncing files at {DateTime.Now}");
            outgoingChannel.TryWrite(new VdrEvent(VdrEventType.FileSync, $"Syncing files at {DateTime.Now}"));
        }

        private async Task IncomingEventsTimer_Elapsed()
        {
            _incomingEventsTimer.Stop();

            await foreach(var item in incommingChannel.ReadAllAsync())
            {
                Console.WriteLine($"Incoming event: {item}");
            }
        }

            public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Background File Sync Service Stopping...");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
    }
}
