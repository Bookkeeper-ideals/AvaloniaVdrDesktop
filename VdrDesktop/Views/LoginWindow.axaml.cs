using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using ReactiveUI;

using System;
using System.Reactive;
using System.Threading.Tasks;

using VdrDesktop.Infrastructure;
using VdrDesktop.Models;
using VdrDesktop.ViewModels;

namespace VdrDesktop;

public partial class LoginWindow : Window
{
    private readonly LoginWindowViewModel _viewModel = new();
    private readonly AuthenticationClient _authenticationClient;
    private readonly SyncSettings _syncSettings;
    private readonly JsonStorage _jsonStorage;

    public ReactiveCommand<Unit, Unit> OnLoginSuccessCommand { get; }

    public LoginWindow()
    {
        this.DataContext = _viewModel;

        InitializeComponent();
    }

    public LoginWindow(AuthenticationClient authenticationClient, SyncSettings syncSettings, JsonStorage jsonStorage)
    {
        _authenticationClient = authenticationClient;
        _syncSettings = syncSettings;
        _jsonStorage = jsonStorage;

        _viewModel.SignInCommand.Subscribe(async _ => await OnLoginAsync());
        OnLoginSuccessCommand = ReactiveCommand.CreateFromTask(async _ => { });

        this.DataContext = _viewModel;

        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private async Task OnLoginAsync()
    {
        if (string.IsNullOrEmpty(_viewModel.UserName) || string.IsNullOrEmpty(_viewModel.Password))
            return;
        var token = await _authenticationClient.AuthenticateAsync(_viewModel.UserName, _viewModel.Password);

        if (token != null)
        {
            _syncSettings.AuthToken = token;
            _syncSettings.UserName = _viewModel.UserName;

            await _jsonStorage.SaveConfigAsync(_syncSettings);
            this.Close();

            OnLoginSuccessCommand.Execute().Subscribe();
        }
        else 
            _viewModel.IsError = true;
    }
}