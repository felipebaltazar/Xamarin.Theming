using Xamarin.Forms;
using Xamarin.Theming.Styles;

namespace Xamarin.Theming
{
    public static class GerenciadorDeTemas
    {
        private static TemaTipo TemaAtual = TemaTipo.Claro;

        public static void DefinirTema(TemaTipo tema)
        {
            switch (tema)
            {
                case TemaTipo.Escuro:
                    DefinirResources<TemaEscuro>();
                    break;
                case TemaTipo.Claro:
                default:
                    DefinirResources<TemaClaro>();
                    break;
            }

            TemaAtual = tema;
        }

        private static void DefinirResources<T>() where T : ResourceDictionary, new()
        {
            if (App.Current.Resources is T)
                return;

            var novoTema = new T();
            App.Current.Resources = novoTema;
        }
    }

    public enum TemaTipo
    {
        Escuro = 0,
        Claro = 1
    }
}
