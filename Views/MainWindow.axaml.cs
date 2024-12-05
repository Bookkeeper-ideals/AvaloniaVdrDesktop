using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

using System;
using System.Linq;
using System.Threading.Tasks;

using VdrDesktop.ViewModels;

namespace VdrDesktop.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel = new();

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

            viewModel.SelectFolderCommand.Set(SelectFolderAsync);
        }

        private async Task SelectFolderAsync()
        {
            //await Dispatcher.UIThread.InvokeAsync(async () =>
            //{
                var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { Title = "Select Folder" });
                if (result.Any())
                {
                    Console.WriteLine($"Selected Folder: {result.First().Path.LocalPath}");
                    //_viewModel.AddEvent($"Selected Folder: {result.First().Path.LocalPath}"); 
                }
            //});
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}