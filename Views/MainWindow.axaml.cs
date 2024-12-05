using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using VdrDesktop.ViewModels;

namespace VdrDesktop.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = new MainWindowViewModel();
            this.AttachDevTools();
        }

        public MainWindow(MainWindowViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}