using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

using Cirrious.MvvmCross.Localization;

namespace bstrkr.mvvm.localization
{
	public class ResxTextProvider : IMvxTextProvider
	{
		private readonly ResourceManager _resourceManager;

		public ResxTextProvider(ResourceManager resourceManager)
		{
			_resourceManager = resourceManager;
			this.CurrentLanguage = CultureInfo.CurrentUICulture;
		}

		public CultureInfo CurrentLanguage { get; set; }

		public string GetText(string namespaceKey, string typeKey, string name)
		{
			string resolvedKey = name;

			if (!string.IsNullOrEmpty(typeKey))
			{
				resolvedKey = string.Format("{0}.{1}", typeKey, resolvedKey);
			}

			if (!string.IsNullOrEmpty(namespaceKey))
			{
				resolvedKey = string.Format("{0}.{1}", namespaceKey, resolvedKey);
			}

			return _resourceManager.GetString(resolvedKey, CurrentLanguage);
		}

		public string GetText(string namespaceKey, string typeKey, string name, params object[] formatArgs)
		{
			string baseText = this.GetText(namespaceKey, typeKey, name);

			if (string.IsNullOrEmpty(baseText))
			{
				return baseText;
			}

			return string.Format(baseText, formatArgs);
		}
	}
}