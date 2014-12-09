using System;
using System.Collections.Generic;

using Cirrious.MvvmCross.Plugins.JsonLocalisation;

namespace bstrkr.mvvm.localisation
{
	public class TextProviderBuilder : MvxTextProviderBuilder
	{
		public TextProviderBuilder()
		{
		}

		protected override IDictionary<string, string> ResourceFiles
		{
			get
			{
//				var dictionary = this.GetType()
//					.Assembly
//					.CreatableTypes()
//					.Where(t => t.Name.EndsWith("ViewModel"))
//					.ToDictionary(t => t.Name, t => t.Name);
//
//				return dictionary;

				return null;
			}
		}
	}
}