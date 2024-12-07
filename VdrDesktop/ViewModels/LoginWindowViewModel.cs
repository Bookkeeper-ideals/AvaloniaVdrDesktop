
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;

namespace VdrDesktop.ViewModels
{
    public class LoginWindowViewModel : ViewModelBase
    {
        private string _userName = string.Empty;
        private string _password = string.Empty;
        private bool _IsError = false;

        public ReactiveCommand<Unit, Unit> SignInCommand { get; }

        public LoginWindowViewModel()
        {
            SignInCommand = ReactiveCommand.CreateFromTask(async _ => { IsError = false; });
        }

        public string UserName
        {
            get { return _userName; }
            set { RaiseAndSetIfChanged(ref this._userName, value); }
        }

        public string Password
        {
            get { return _password; }
            set { RaiseAndSetIfChanged(ref this._password, value); }
        }

        public bool IsError
        {
            get { return _IsError; }
            set { RaiseAndSetIfChanged(ref this._IsError, value); }
        }
    }
}
