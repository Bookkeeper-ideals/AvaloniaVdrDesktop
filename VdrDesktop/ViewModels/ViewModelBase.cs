using CommunityToolkit.Mvvm.ComponentModel;

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace VdrDesktop.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        private PropertyChangedEventHandler? _propertyChanged;
        private List<string> events = new List<string>();

        protected bool RaiseAndSetIfChanged<T>([NotNullIfNotNull("value")] ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                RaisePropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            OnPropertyChanged(e);
            _propertyChanged?.Invoke(this, e);
        }
    }
}
