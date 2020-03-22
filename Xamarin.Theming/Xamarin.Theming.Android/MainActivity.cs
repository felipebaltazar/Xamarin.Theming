
using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;

namespace Xamarin.Theming.Droid
{
    [Activity(Label = "Xamarin.Theming", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            this.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            LoadApplication(new App());
            PegarTemaAtual(Resources.Configuration);
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            var temaAtual = PegarTemaAtual(newConfig);
            GerenciadorDeTemas.DefinirTema(temaAtual);
        }

        private static TemaTipo PegarTemaAtual(Configuration newConfig)
        {
            if ((newConfig.UiMode & UiMode.NightNo) != 0)
                return TemaTipo.Claro;

            return TemaTipo.Escuro;
        }
    }
}