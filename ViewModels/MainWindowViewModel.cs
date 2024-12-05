using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace VdrDesktop.ViewModels
{
    public class EventItem 
    {
        public string Text { get; set; }
    }
    public partial class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<EventItem> Events { get; }

        public string[] Tests => new string[] { "Test 1", "Test 2", "Test 3" };

        public MainWindowViewModel()
        {
            Events = new ObservableCollection<EventItem>(
                Enumerable.Range(0, 20).Select(x => new EventItem { Text = "Item " + x + " details" }));
            //OnPropertyChanged(nameof(Events));

        }
    }
}
