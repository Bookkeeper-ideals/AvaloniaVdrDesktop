using FileSyncUtility.Infrastructure;
using FileSyncUtility.Model;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using VdrDesktop.Models;

namespace VdrDesktop
{
    public class BackgroundFileSyncService(IConfiguration configuration, SyncSettings syncSettings, ChannelWriter<VdrEvent> outgoingChannel, ChannelReader<VdrEvent> incommingChannel,
        Func<SynchronizationProcess> syncProcessCreate) : IHostedService
    {
        private readonly ConcurrentDictionary<string, SynchronizationProcess> _synchronizers = new();

        private System.Timers.Timer _incomingEventsTimer;

        private Channel<string> _channel = Channel.CreateUnbounded<string>();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Background File Sync Service Starting...");

            if(!Directory.Exists(configuration.GetValue<string>("GlobalSyncFolder")))
                Directory.CreateDirectory(configuration.GetValue<string>("GlobalSyncFolder"));

            _incomingEventsTimer = new System.Timers.Timer(500);
            _incomingEventsTimer.Elapsed += async (sender, e) => await IncomingEventsTimer_Elapsed();
            _incomingEventsTimer.AutoReset = true; // Repeat the timer event
            _incomingEventsTimer.Enabled = true; // Start the timer

            foreach (var folder in syncSettings.Folders)
                AddSynchronizer(folder);

            return Task.CompletedTask;
        }

        private async Task IncomingEventsTimer_Elapsed()
        {
            _incomingEventsTimer.Stop();

            await foreach (var item in incommingChannel.ReadAllAsync())
            {
                if(item.EventType == VdrEventType.FolderAddToWatch)
                    AddSynchronizer(item.Message);

                if(item.EventType == VdrEventType.FolderRemoveFromWatch)
                    RemoveSynchronizer(item.Message);
            }
        }

        public void AddSynchronizer(string folder)
        {
            if (_synchronizers.ContainsKey(folder))
                return;

            string folderName = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar))!;
            string globalSyncPath = Path.Combine(configuration.GetValue<string>("GlobalSyncFolder"), folderName);

            if (!Directory.Exists(globalSyncPath))
                Directory.CreateDirectory(globalSyncPath);

            var syncProcess = syncProcessCreate();

            syncProcess.ProcessNotification += BroadcastSyncEvent;

            syncProcess.Start(globalSyncPath, folder);
            _synchronizers.TryAdd(folder, syncProcess);
        }

        public void RemoveSynchronizer(string folder)
        {
            if (_synchronizers.TryRemove(folder, out var syncProcess))
            {
                syncProcess.ProcessNotification -= BroadcastSyncEvent;
                syncProcess.Dispose();

                try
                {
                    string folderName = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar))!;
                    string globalSyncPath = Path.Combine(configuration.GetValue<string>("GlobalSyncFolder"), folderName);

                    if (Directory.Exists(globalSyncPath))
                        Directory.Delete(globalSyncPath, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing folder: {ex.Message}");
                }
            }
        }

        public void BroadcastSyncEvent(Object? sender, SyncNotification e)
        {
            outgoingChannel.TryWrite(new VdrEvent(VdrEventType.FileSync, $"{e.Type}: {e.Message}"));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var sync in _synchronizers)
            {
                sync.Value.ProcessNotification -= BroadcastSyncEvent;
                sync.Value.Dispose();
            }

            Console.WriteLine("Background File Sync Service Stopping...");
            return Task.CompletedTask;
        }
    }
}
