using System;

using bstrkr.core;
using bstrkr.core.context;

namespace bstrkr.mvvm.viewmodels
{
	public class AboutViewModel : BusTrackerViewModelBase
	{
		public string AboutText
		{
			get 
			{ 
				return string.Format(AppResources.about_view_text, BusTrackerAppContext.Version); 
			}
		}
	}
}