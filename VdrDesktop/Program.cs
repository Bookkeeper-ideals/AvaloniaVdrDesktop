using Avalonia;
using Avalonia.Labs.Notifications;
using Avalonia.ReactiveUI;

using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Net.Http;

using VdrDesktop.Infrastructure;

namespace VdrDesktop
{
    internal sealed class Program
    {
        public static IConfiguration Configuration { get; private set; } = null!;
        public static AuthenticationClient AuthenticationClient { get; private set; } = null!;
        private static MockAuthenticationServer _mockServer = new MockAuthenticationServer();

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // Start the mock server
            
            int port = _mockServer.Start();
            string tokenUrl = $"http://localhost:{port}/oauth/token";

            // Create and configure the authentication client
            var httpClient = new HttpClient();
            AuthenticationClient = new AuthenticationClient(httpClient, tokenUrl);

            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>(() => new App(Configuration, AuthenticationClient))
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
                        new NotificationChannel("open", "Send Notification with Open Action", NotificationPriority.High)
                        {
                            Actions = new List<NativeNotificationAction>
                            {
                                new NativeNotificationAction("Open", "open")
                            }
                        },
                    }
                })
                .UseReactiveUI()
                .WithInterFont()
                .LogToTrace();
    }
}
