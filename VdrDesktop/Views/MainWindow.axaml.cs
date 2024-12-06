using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using VdrDesktop.ViewModels;

namespace VdrDesktop.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel = new();

        public MainWindowViewModel ViewModel => _viewModel;

        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = _viewModel;
        }

        public MainWindow(MainWindowViewModel viewModel)
        {
            this.InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = viewModel;

            viewModel.SelectFolderCommand.Subscribe(async _ => await SelectFolderAsync());

            ClientSize = new Avalonia.Size(900, 450);
            CanResize = false;
        }

        private async Task SelectFolderAsync()
        {
            var results = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Folder",
                AllowMultiple = true
            });

            foreach (var result in results)
                _viewModel.AddFolder(result.Path.LocalPath);

            _viewModel.FolderSelectedCommand.Execute(results.Select(x => x.Path.LocalPath).ToArray()).Subscribe();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}