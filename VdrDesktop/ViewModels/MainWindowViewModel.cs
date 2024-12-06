using ReactiveUI;

using System;
using System.Collections.Generic;
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

        public ReactiveCommand<string, string> RemoveFolderCommand { get; }

        public ReactiveCommand<IEnumerable<string>, IEnumerable<string>> FolderSelectedCommand { get; }


        public MainWindowViewModel() 
        {
            
        }

        public MainWindowViewModel(ChannelWriter<VdrEvent> outgoingChannel) 
        {
            SelectFolderCommand = ReactiveCommand.CreateFromTask(async _ => { });
            RemoveFolderCommand = ReactiveCommand.CreateFromTask<string, string>(RemoveFolderAsync);
            FolderSelectedCommand = ReactiveCommand.CreateFromTask<IEnumerable<string>, IEnumerable<string>>(async folders => folders);
            _channel = outgoingChannel;
        }

        public void AddFolder(string folder)
        {
            if (Folders.Select(x => x.Text).Contains(folder, StringComparer.InvariantCultureIgnoreCase))
                return;

            Folders.Insert(0, new ListItem { Text = folder });
            _channel?.TryWrite(new VdrEvent(VdrEventType.FolderAddToWatch, folder));
        }

        public async Task<string> RemoveFolderAsync(string folder)
        {            
            var item = Folders.FirstOrDefault(x => x.Text == folder);
            if (item is null)
                return null;

            Folders.Remove(item);
            _channel?.TryWrite(new VdrEvent(VdrEventType.FolderRemoveFromWatch, folder));

            return folder;
        }
    }
}
