using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Theming.Extensions
{
	[ContentProperty(nameof(Source))]
	public sealed class ImageResourceExtension : IMarkupExtension
	{
		public string Source { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider) =>
			Source != null ? ImageSource.FromResource(Source) : null;
	}
}
