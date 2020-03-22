using System.ComponentModel;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Theming.Extensions;
using Xamarin.Theming.ViewModels;

namespace Xamarin.Theming.Views
{
    public partial class LoginPage : ContentPage
    {
        #region Fields

        private const int LOADING_WIDTH = 50;
        private TaskCompletionSource<bool> loadingAnimationTask;

        #endregion

        #region Constructors

        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginPageViewModel();
        }

        #endregion

        #region Overrides

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SubscribeEvents();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            UnsubscribeEvents();
        }

        #endregion

        #region Private Methods

        private void SubscribeEvents()
        {
            MessagingCenter
                .Subscribe<LoginPageViewModel, bool>(this, "OnLoginActionCallback", OnLoginAuthentication);
        }

        /// Nunca se esqueca de desinscrever os eventos
        private void UnsubscribeEvents()
        {
            MessagingCenter
                .Unsubscribe<LoginPageViewModel, bool>(this, "OnLoginActionCallback");
        }

        // Quando recebermos o evento de callback do login do MessagingCenter
        // verificaremos o seu resultado para fazer a animaçao correta
        private void OnLoginAuthentication(LoginPageViewModel sender, bool success)
        {
            // Manipulaçoes de View devem sempre ocorrer na UI Thread
            // dessa forma vamos garantir que isso aconteça dessa forma
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (success)
                    await OnSuccessLoginAsync();
                else
                    await OnErrorLoadingAsync();
            });
        }

        private async void OnActivityIndicatorPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Quando a propriedade `IsRunning` do activity indicator for true,
            // vamos iniciar a animaçao de loading

            if (!e.PropertyName.Equals(nameof(ActivityIndicator.IsRunning))) return;
            if (!((ActivityIndicator)sender).IsRunning) return;

            await ShowLoadingAsync();
        }

        // Aqui fazemos a animaçao do botao se tornar o ActivityIndicator
        private async Task ShowLoadingAsync()
        {
            // Aqui definimos que a animação de loading vai começar
            // e deve ser aguardada antes de iniciar outra animaçao
            loadingAnimationTask?.TrySetCanceled();
            loadingAnimationTask = new TaskCompletionSource<bool>();

            try
            {
                // Tornamos visivel o frame do login que usa o mesmo tamanho/formato do botão
                // Utilizei aqui o Frame, ao inves de manipular diretamente o botão porque achei mais facil
                // de alterar sem precisar ocultar texto ou outrar propriedaes do botão
                LoginFrame.IsVisible = true;

                // Criamos uma animaçao onde o frame vai ficar com a cor de fundo igual a do botao
                // e o botão faz um Fade, ficando totalmente transparente, ficando assim
                // apenas visivel o frame que contém o ActivityIndicator dentro
                _ = await Task.WhenAll(
                    LoginFrame.BackgroundColorTo(LoginButton.BackgroundColor, 50),
                    LoginButton.FadeTo(0, 100));

                // Calculamos a nova posição do frame, após reduzir a largura dele para 50
                var anchorX = (LoginFrame.Width / 2) - (LOADING_WIDTH / 2);

                // Criamos então a posição/tamanho final do frame (layout)
                var rectTo = new Rectangle(anchorX, LoginFrame.Y, LOADING_WIDTH, LoginFrame.Height);

                // Fazemos a transição que vai manipular a posicão/tamanho do frame
                _ = await LoginFrame.LayoutTo(rectTo, easing: Easing.SpringOut);
            }
            finally
            {
                // Por fim definimos que a animação de loading foi finalizada
                loadingAnimationTask.SetResult(true);
            }
        }

        /// Caso o login seja efetuado com sucesso iremos transitar para a MainPage
        private async Task OnSuccessLoginAsync()
        {
            // Caso seu servidor responda muito rápido, pode ser que a
            // animação ainda esteja sendo executada, por isso
            // utilizamos o TaskCompletationSource para aguardar o término da animaçao de loading
            _ = await loadingAnimationTask.Task;

            // Aqui a ideia é saber dinamicamente o quão deslocado do ponto zero, da ContentPage,
            // O LoginFrame está, para a gente conseguir deslocá-lo até o canto superior esquerdo da tela
            // Para isso calculamos sua posição absoluta
            (var absX, var absY) = ((VisualElement)LoginFrame.Parent).GetAbsolutePosition();

            // Criamos entao a posiçao final, reduzindo toda a posiçao absoluta do frame e
            // o tamanho total, que será o mesmo tamanho da página
            var rectTo = new Rectangle(-absX, -absY, this.Width, this.Height);

            // Buscamos, nos resources, a cor de fundo que aplicamos na MainPage
            // para simular que o botão está se tornando a proxima página
            var mainPageBackgroundColor = (Color)Application.Current.Resources["CorDeFundo"];

            // Criamos a transiçao do raio do frame + o tamanh/posição (Layout) + a cor defundo
            // E aguardamos a execução de todas
            _ = await Task.WhenAll(
                LoginFrame.CornerRadiusTo(0, 500),
                LoginFrame.LayoutTo(rectTo, 500, Easing.SpringOut),
                LoginFrame.BackgroundColorTo(mainPageBackgroundColor, 1000));

            // Por fim executamos a navegaçao para a MainPage
            await Navigation.PushAsync(new MainPage());
        }

        /// Caso dê algum erro na autenticação, devemos voltar
        /// para o estado original das views, assim podemos
        /// fazer uma animação de retorno
        private async Task OnErrorLoadingAsync()
        {
            // Caso seu servidor responda muito rápido, pode ser que a
            // animação ainda esteja sendo executada, por isso
            // utilizamos o TaskCompletationSource para aguardar o término da animaçao de loading
            _ = await loadingAnimationTask.Task;

            // Voltamos a visibilidade do botao de login
            _ = await LoginButton.FadeTo(1, 100);

            // Deixaremos novamente o Frame no tamanho/posicão exatos do login
            var rectTo = new Rectangle(0, LoginButton.Y, LoginButton.Width, LoginButton.Height);

            // Criamos a transiçao do raio do frame + o tamanh/posição (Layout) + a cor defundo
            // E aguardamos a execução de todas
            _ = await Task.WhenAll(
                LoginFrame.CornerRadiusTo(LoginButton.CornerRadius),
                LoginFrame.LayoutTo(rectTo, 500, Easing.SpringOut),
                LoginFrame.BackgroundColorTo(Color.Transparent, 1000));

            // Finalizamos deixando o Frame Invisível
            // Pode parecer redundante, pois deixamos o fundo transparente
            // Mas parece que o comportamento do `IsVisible` não cooperou pra mim no IOS
            // Pode ser um bug na versão atual do Xamarin.Forms, ainda vou investigar mais
            LoginFrame.IsVisible = false;
        }

        #endregion
    }
}