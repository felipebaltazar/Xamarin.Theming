using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Xamarin.Theming.Extensions
{
    internal static class ViewExtensions
    {
        #region Propriedades com Dynamic Setters

        /// Verifica se uma propriedade tem algum Dynamic associado à ela
        /// Como os Setters de estilos, por exemplo
        internal static bool HasDynamicColorOnProperty<TView>(this TView view, BindableProperty property) where TView : View
        {
            var resourceValue = view?.GetPropertyDynamicResourceValue(property);
            if (resourceValue == null) return false;
            var currentValue = GetValueFromPropertyName(view, property.PropertyName);
            return currentValue != null && (Color)currentValue == (Color)resourceValue;
        }

        /// Busca o valor do recurso dinamico associado à uma propriedade
        internal static object GetPropertyDynamicResourceValue<TView>(this TView view, BindableProperty property) where TView : View
        {
            var resourceKey = view.GetPropertyDynamicResourceKey(property);
            if (string.IsNullOrWhiteSpace(resourceKey))
                return null;
            var currentResources = Application.Current.Resources;
            if (!currentResources.TryGetValue(resourceKey, out var resourceValue))
                return null;
            return resourceValue;
        }

        /// Busca a chave do recurso dinamico associado à uma propriedade
        internal static string GetPropertyDynamicResourceKey<TView>(this TView view, BindableProperty propertyToCheck) where TView : View
        {
            var elementStyle = view?.Style;
            var styleSetters = GetAllSetters(elementStyle);
            var setter = styleSetters?.FirstOrDefault(
                s => s.Property.PropertyName == propertyToCheck.PropertyName);
            if (setter?.Value is DynamicResource dynamicResource)
                return dynamicResource.Key;
            return null;
        }

        /// Define o recurso dinamico na propriedade se existir, caso contrário
        /// usa o defaultSetter para definir um valor padrão
        internal static void SetDynamicOrDefaultValue<TView>(this TView view,
            BindableProperty propertyToCheck, Action<TView> defaultSetter) where TView : View
        {

            var resourceKey = view.GetPropertyDynamicResourceKey(propertyToCheck);
            if (string.IsNullOrWhiteSpace(resourceKey))
            {
                defaultSetter?.Invoke(view);
            }
            else
            {
                view.SetDynamicResource(propertyToCheck, resourceKey);
            }
        }

        /// Busca todos os Setters de um estilo, buscando nas herancas (recursivo) 
        private static IEnumerable<Setter> GetAllSetters(Style style)
        {
            if (style == null)
                return Enumerable.Empty<Setter>();

            return style.Setters.Concat(GetAllSetters(style.BasedOn));
        }

        /// Pega o valor de uma propriedade via reflection
        private static object GetValueFromPropertyName(object view, string propertyToCheck) =>
            view.GetType().GetProperty(propertyToCheck).GetValue(view);

        #endregion

        #region Animações

        /// <summary>
        /// Efetua uma animaçao no CornerRadius de um frame
        /// </summary>
        internal static Task<bool> CornerRadiusTo(this Frame frame, float toRadius, uint length = 250, Easing easing = null)
        {
            var fromRadius = frame.CornerRadius;
            float transform(double t) => (float)(fromRadius + t * (toRadius - fromRadius));

            return CornerRadiusAnimation(frame, nameof(CornerRadiusTo), transform, length, easing);
        }

        /// <summary>
        /// Efetua uma animaçao de transição de cor, no background de um visualelement
        /// </summary>
        internal static Task<bool> BackgroundColorTo(this VisualElement element, Color toColor, uint length = 250, Easing easing = null)
        {
            var fromColor = element.BackgroundColor;
            Color transform(double t) =>
                Color.FromRgba(fromColor.R + t * (toColor.R - fromColor.R),
                               fromColor.G + t * (toColor.G - fromColor.G),
                               fromColor.B + t * (toColor.B - fromColor.B),
                               fromColor.A + t * (toColor.A - fromColor.A));

            return BackgroundColorAnimation(element, nameof(BackgroundColorTo), transform, length, easing);
        }

        internal static void CancelCornerRadiusAnimation(this Frame frame) =>
            frame.AbortAnimation(nameof(CornerRadiusTo));

        internal static void CancelBackgroundColorAnimation(this VisualElement element) =>
            element.AbortAnimation(nameof(BackgroundColorTo));

        private static Task<bool> CornerRadiusAnimation(Frame frame, string name,
            Func<double, float> transform, uint length, Easing easing)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            frame.Animate(
                name,
                transform,
                (radius) => frame.CornerRadius = radius,
                16,
                length,
                easing ?? Easing.Linear,
                (v, c) => taskCompletionSource.SetResult(c));

            return taskCompletionSource.Task;

        }

        private static Task<bool> BackgroundColorAnimation(VisualElement element, string name,
            Func<double, Color> transform, uint length, Easing easing)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            element.Animate(
                name,
                transform,
                (color) => element.BackgroundColor = color,
                16,
                length,
                easing ?? Easing.Linear,
                (v, c) => taskCompletionSource.SetResult(c));

            return taskCompletionSource.Task;
        }

        #endregion

        /// <summary>
        /// Busca a posição absoluta de uma view
        /// </summary>
        public static (double X, double Y) GetAbsolutePosition(this VisualElement view)
        {
            var screenCoordinateX = view.X;
            var screenCoordinateY = view.Y;

            if (view.Parent.GetType() != typeof(App))
            {
                var parent = (VisualElement)view.Parent;

                while (parent != null)
                {
                    screenCoordinateX += parent.X;
                    screenCoordinateY += parent.Y;

                    if (parent.Parent.GetType() == typeof(App))
                        parent = null;
                    else
                        parent = (VisualElement)parent.Parent;
                }
            }

            return (screenCoordinateX, screenCoordinateY);
        }
    }
}
