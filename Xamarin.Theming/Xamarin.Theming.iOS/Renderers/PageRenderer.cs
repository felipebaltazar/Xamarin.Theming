using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ContentPage), typeof(Xamarin.Theming.iOS.Renderers.PageRenderer))]
namespace Xamarin.Theming.iOS.Renderers
{
    public class PageRenderer : Xamarin.Forms.Platform.iOS.PageRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
                return;

            try
            {
                SetAppTheme();
            }
            catch (Exception ex)
            {
                //Crie seus logs de erro aqui
            }
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            if (TraitCollection.UserInterfaceStyle != previousTraitCollection.UserInterfaceStyle)
            {
                SetAppTheme();
            }
        }

        private void SetAppTheme()
        {
            var temaAtual = PegarTemaAtual();
            GerenciadorDeTemas.DefinirTema(temaAtual);
        }

        private TemaTipo PegarTemaAtual()
        {
            if (TraitCollection?.UserInterfaceStyle == UIUserInterfaceStyle.Dark)
                return TemaTipo.Escuro;

            return TemaTipo.Claro;
        }
    }
}