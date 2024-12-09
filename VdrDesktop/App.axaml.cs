using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Labs.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;

using FileSyncUtility.Infrastructure;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using VdrDesktop.Infrastructure;
using VdrDesktop.Models;
using VdrDesktop.ViewModels;
using VdrDesktop.Views;

namespace VdrDesktop
{
    public partial class App : Application
    {
        private IHost? _host;
        private Window? _mainWindow;
        private LoginWindow? _loginWindow;
        private TrayIcon? _trayIcon;

        private readonly IConfiguration _configuration;
        private readonly AuthenticationClient _authenticationClient;

        private System.Timers.Timer _incomingEventsTimer = new System.Timers.Timer(500);

        private SyncSettings _syncSettings;
        private JsonStorage _jsonStorage;

        private readonly MainWindowViewModel _mainWindowViewModel;

        private Channel<VdrEvent> _backgroundFileSyncServiceChannel = Channel.CreateUnbounded<VdrEvent>();
        private Channel<VdrEvent> _guiChannel = Channel.CreateUnbounded<VdrEvent>();

        private Bitmap? _icon;

        public App(IConfiguration configuration, AuthenticationClient authenticationClient)
        {
            _configuration = configuration;
            _authenticationClient = authenticationClient;

            _jsonStorage = new JsonStorage("syncConfig.json");

            _mainWindowViewModel = new MainWindowViewModel(_guiChannel.Writer);            
            _mainWindowViewModel.FolderSelectedCommand.Subscribe(async folders => await OnFolderSelectedAsync(folders));
            _mainWindowViewModel.RemoveFolderCommand.Subscribe(async folder => await OnRemoveFolderAsync(folder));
        }

        private async Task OnFolderSelectedAsync(IEnumerable<string> folders)
        {
            if (!folders.Any())
                return;
            
            var foldersToStore = folders.Except(_syncSettings.Folders).ToArray().Reverse();

            _syncSettings.Folders = [.. foldersToStore, .. _syncSettings.Folders];
            await _jsonStorage.SaveConfigAsync(_syncSettings);
        }

        private async Task OnRemoveFolderAsync(string folder)
        {
            _syncSettings.Folders = _syncSettings.Folders.Except([folder]).ToArray();
            await _jsonStorage.SaveConfigAsync(_syncSettings);
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            _icon = new Bitmap(AssetLoader.Open(new System.Uri("avares://VdrDesktop/Assets/vdr.ico")));

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _syncSettings = _jsonStorage.LoadConfig() ?? new SyncSettings();

                foreach (var folder in _syncSettings.Folders)
                    _mainWindowViewModel.Folders.Add(new ListItem { Text = folder });

                _mainWindowViewModel.UserName = _syncSettings.UserName;

                // Configure the host with the background service
                _host = Host.CreateDefaultBuilder()
                            .ConfigureServices((_, services) =>
                            {
                                services
                                .AddSingleton(_configuration)
                                .AddTransient<ISyncEventsTracking, FileSystemTracking>()
                                .AddTransient<IStorageActions, UserStorage>()
                                .AddTransient<SynchronizationProcess>()
                                .AddSingleton<Func<SynchronizationProcess>>(provider => () => provider.GetRequiredService<SynchronizationProcess>())
                                .AddHostedService(provider => new BackgroundFileSyncService(_configuration, _syncSettings, _backgroundFileSyncServiceChannel.Writer, 
                                    _guiChannel.Reader, provider.GetRequiredService<Func<SynchronizationProcess>>()));
                            })
                            .Build();

                // Start the host
                _host.Start();

                // Prevent the app from shutting down when the main window is closed
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                // Add the system tray icon
                _trayIcon = new TrayIcon
                {
                    Icon = new WindowIcon(_icon),
                    ToolTipText = "VdrDesktop"
                };

                _trayIcon.Menu = BuildTrayIconMenu();
                _trayIcon.Clicked += (_, _) => ShowActualWindow();
                _trayIcon.IsVisible = true;                

                _mainWindowViewModel.Events.Add(new ListItem { Text = "Application started" });

                _incomingEventsTimer.Elapsed += async (sender, e) => await IncomingEventsTimer_Elapsed();
                _incomingEventsTimer.AutoReset = true; // Repeat the timer event
                _incomingEventsTimer.Enabled = true; // Start the timer

                if(NativeNotificationManager.Current is not null)
                    NativeNotificationManager.Current.NotificationCompleted += NotificationOnOpen;

                desktop.Exit += async (_, _) =>
                {
                    await _host!.StopAsync();
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private NativeMenu BuildTrayIconMenu()
        {
            var menu = new NativeMenu();

            var desktop = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

            var exitMenuItem = new NativeMenuItem("Exit");
            exitMenuItem.Click += (_, _) => desktop.Shutdown();

            if (string.IsNullOrEmpty(_syncSettings.AuthToken))
            {
                var signInMenuItem = new NativeMenuItem("Sign In");
                signInMenuItem.Click += (_, _) => ShowLoginWindow();
                menu.Items.Add(signInMenuItem);
                menu.Items.Add(new NativeMenuItemSeparator());
            }
            else
            {
                var mainWindowMenuItem = new NativeMenuItem("Show Main Window");
                mainWindowMenuItem.Click += (_, _) => ShowMainWindow();
                menu.Items.Add(mainWindowMenuItem);

                menu.Items.Add(new NativeMenuItemSeparator());

                var signOutMenuItem = new NativeMenuItem("Sign Out");
                signOutMenuItem.Click += async (_, _) => await SignOutAsync();
                menu.Items.Add(signOutMenuItem);

                menu.Items.Add(new NativeMenuItemSeparator());
            }           

            menu.Items.Add(exitMenuItem);

            return menu;
        }

        public void ShowActualWindow()
        { 
            if(string.IsNullOrEmpty(_syncSettings.UserName))
                ShowLoginWindow();
            else
                ShowMainWindow();
        }

        private async Task IncomingEventsTimer_Elapsed()
        {
            _incomingEventsTimer.Stop();

            await foreach (var item in _backgroundFileSyncServiceChannel.Reader.ReadAllAsync())
            {
                _mainWindowViewModel?.Events.Insert(0, new ListItem { Text = $"{item.EventType}: {item.Message}" });

                if (NativeNotificationManager.Current?.CreateNotification("open") is var notification && notification is not null)
                {
                    notification.Message = item.Message;
                    notification.Title = item.EventType.ToString();
                    notification.Icon = _icon;
                    notification.Expiration = TimeSpan.FromSeconds(5);
                    notification.Show();
                }
            }
        }

        private void NotificationOnOpen(object? sender, NativeNotificationCompletedEventArgs args)
        { 
            if(args.ActionTag == "open")
                ShowMainWindow();
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

        private void ShowLoginWindow()
        {
            if (_loginWindow == null)
            {
                _loginWindow = new LoginWindow(_authenticationClient, _syncSettings, _jsonStorage);
                _loginWindow.Closed += (_, _) => _loginWindow = null; // Dispose the reference when closed

                _loginWindow.OnLoginSuccessCommand.Subscribe(async _ =>
                {
                    _trayIcon.Menu = BuildTrayIconMenu();
                    _mainWindowViewModel.UserName = _syncSettings.UserName;
                    ShowMainWindow();
                });
            }

            _loginWindow.Show();
            _loginWindow.Activate(); // Bring the window to the foreground
        }

        private async Task SignOutAsync()
        {
            _syncSettings.AuthToken = null;
            _syncSettings.UserName = null;
            await _jsonStorage.SaveConfigAsync(_syncSettings);

            _mainWindowViewModel.UserName = string.Empty;

            _trayIcon.Menu = BuildTrayIconMenu();
            _mainWindow?.Close();
            ShowLoginWindow();
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