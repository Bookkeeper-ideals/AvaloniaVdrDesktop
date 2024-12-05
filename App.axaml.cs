using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

using VdrDesktop.ViewModels;
using VdrDesktop.Views;

namespace VdrDesktop
{
    public partial class App : Application
    {
        private IHost? _host;
        private Window? _mainWindow;
        private TrayIcon? _trayIcon;

        private System.Timers.Timer _incomingEventsTimer = new System.Timers.Timer(500);

        private readonly MainWindowViewModel _mainWindowViewModel = new();

        private Channel<string> _backgroundFileSyncServiceChannel = Channel.CreateUnbounded<string>();
        private Channel<string> _guiChannel = Channel.CreateUnbounded<string>();

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Configure the host with the background service
                _host = Host.CreateDefaultBuilder()
                            .ConfigureServices((_, services) =>
                            {
                                services.AddHostedService(_ => new BackgroundFileSyncService(_backgroundFileSyncServiceChannel.Writer, _guiChannel.Reader));
                            })
                            .Build();

                // Start the host
                _host.Start();

                // Prevent the app from shutting down when the main window is closed
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                // Add the system tray icon
                _trayIcon = new TrayIcon
                {
                    Icon = new WindowIcon("Assets/trayicon.png"), // Path to your icon
                    ToolTipText = "SystemTray Application"
                };

                // Add context menu to tray icon

                var mainWindowMenuItem = new NativeMenuItem("Show Main Window");
                    mainWindowMenuItem.Click += (_, _) => ShowMainWindow();

                var exitMenuItem = new NativeMenuItem("Exit");
                    exitMenuItem.Click += (_, _) => desktop.Shutdown();

                _trayIcon.Menu = new NativeMenu();
                _trayIcon.Menu.Items.Add(mainWindowMenuItem);
                _trayIcon.Menu.Items.Add(new NativeMenuItemSeparator());
                _trayIcon.Menu.Items.Add(exitMenuItem);

                _trayIcon.IsVisible = true;
                _trayIcon.Clicked += (_, _) => ShowMainWindow();

                _mainWindowViewModel.Events.Add(new EventItem { Text = "Application started" });

                _incomingEventsTimer.Elapsed += async (sender, e) => await IncomingEventsTimer_Elapsed();
                _incomingEventsTimer.AutoReset = true; // Repeat the timer event
                _incomingEventsTimer.Enabled = true; // Start the timer
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task IncomingEventsTimer_Elapsed()
        {
            _incomingEventsTimer.Stop();

            await foreach (var item in _backgroundFileSyncServiceChannel.Reader.ReadAllAsync())
            {
                _mainWindowViewModel.Events.Add(new EventItem { Text = item });
            }
        }

        private void ShowMainWindow()
        {
            if (_mainWindow == null)
            {
                _mainWindow = new MainWindow(_mainWindowViewModel);
                _mainWindow.Closing += OnMainWindowClosing;
                _mainWindow.Closed += (_, _) => _mainWindow = null; // Dispose the reference when closed
            }

            _mainWindow.Show();
            _mainWindow.Activate(); // Bring the window to the foreground
        }

        private void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // Prevent the window from closing
            (sender as Window)?.Hide(); // Hide the window instead
        }

        //private void DisableAvaloniaDataAnnotationValidation()
        //{
        //    // Get an array of plugins to remove
        //    var dataValidationPluginsToRemove =
        //        BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        //    // remove each entry found
        //    foreach (var plugin in dataValidationPluginsToRemove)
        //    {
        //        BindingPlugins.DataValidators.Remove(plugin);
        //    }
        //}
    }
}