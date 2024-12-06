﻿using Avalonia;
using Avalonia.Labs.Notifications;
using Avalonia.ReactiveUI;

using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;

namespace VdrDesktop
{
    internal sealed class Program
    {
        public static IConfiguration Configuration { get; private set; } = null!;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>(() => new App(Configuration))
                .UsePlatformDetect()
                .WithAppNotifications(new AppNotificationOptions()
                {
                    Channels = new[]
                    {
                        new NotificationChannel("basic", "Send Notifications", NotificationPriority.High),
                        new NotificationChannel("actions", "Send Notification with Predefined Actions", NotificationPriority.High)
                        {
                            Actions = new List<NativeNotificationAction>
                            {
                                new NativeNotificationAction("Hello", "hello"),
                                new NativeNotificationAction("world", "world")
                            }
                        },
                        new NotificationChannel("custom", "Send Notification with Custom Actions", NotificationPriority.High),
                        new NotificationChannel("reply", "Send Notification with Reply Action", NotificationPriority.High)
                        {
                            Actions = new List<NativeNotificationAction>
                            {
                                new NativeNotificationAction("Reply", "reply")
                            }
                        },
                    }
                })
                .UseReactiveUI()
                .WithInterFont()
                .LogToTrace();
    }
}
