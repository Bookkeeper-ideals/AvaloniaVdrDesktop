using ReactiveUI;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Channels;
using System.Threading.Tasks;

using VdrDesktop.Models;

namespace VdrDesktop.ViewModels
{
    public class ListItem 
    {
        public string Text { get; set; }
    }

    public partial class MainWindowViewModel : ViewModelBase
    {
        private ChannelWriter<VdrEvent>? _channel;
        public ObservableCollection<ListItem> Events { get; } = new();

        public ObservableCollection<ListItem> Folders { get; } = new();

        public ReactiveCommand<Unit, Unit> SelectFolderCommand { get; }

        public ReactiveCommand<string, Unit> RemoveFolderCommand { get; }


        public MainWindowViewModel() 
        {
            
        }

        public MainWindowViewModel(ChannelWriter<VdrEvent> outgoingChannel) 
        {
            SelectFolderCommand = ReactiveCommand.CreateFromTask(SelectFolderAsync);
            RemoveFolderCommand = ReactiveCommand.CreateFromTask<string>(RemoveFolderAsync);
            _channel = outgoingChannel;
        }

        private async Task SelectFolderAsync()
        {
        }

        public void AddFolder(string folder)
        {
            if (Folders.Select(x => x.Text).Contains(folder, StringComparer.InvariantCultureIgnoreCase))
                return;

            Folders.Insert(0, new ListItem { Text = folder });
            _channel?.TryWrite(new VdrEvent(VdrEventType.FolderAddToWatch, folder));
        }

        public async Task RemoveFolderAsync(string folder)
        {            
            var item = Folders.FirstOrDefault(x => x.Text == folder);
            if (item is null)
                return;

            Folders.Remove(item);
            _channel?.TryWrite(new VdrEvent(VdrEventType.FolderRemoveFromWatch, folder));
        }
    }
}
