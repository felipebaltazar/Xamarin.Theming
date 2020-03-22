using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Xamarin.Theming.ViewModels
{
    public sealed class LoginPageViewModel : BaseViewModel
    {
        private string username;
        private string password;

        public ICommand LoginCommand { get; private set; }

        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        public LoginPageViewModel()
        {
            LoginCommand = new Command(LoginCommandExecute);
        }

        private void LoginCommandExecute(object obj)
        {
            if (IsBusy) return;

            Task.Run(async () =>
            {
                IsBusy = true;
                await Task.Delay(2000);

                var authenticated = "UsuarioCorreto".Equals(Username, StringComparison.OrdinalIgnoreCase);

                MessagingCenter.Send(this, "OnLoginActionCallback", authenticated);
                IsBusy = false;
            });
        }
    }
}
