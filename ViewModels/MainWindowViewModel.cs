using Avalonia.Threading;

using MiniMvvm;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Reactive;
using System.Threading.Tasks;

namespace VdrDesktop.ViewModels
{
    public class EventItem 
    {
        public string Text { get; set; }
    }
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<EventItem> Events { get; } = new();
        public Command SelectFolderCommand { get; }


        public MainWindowViewModel() 
        {
            SelectFolderCommand = new Command(SelectFolderAsync);
        }

        private async Task SelectFolderAsync()
        {
        }
    }
}
