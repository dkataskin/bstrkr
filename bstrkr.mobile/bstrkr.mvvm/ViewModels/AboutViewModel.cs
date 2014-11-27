using System;

namespace bstrkr.mvvm.viewmodels
{
	public class AboutViewModel : BusTrackerViewModelBase
	{
		private string _aboutText;

		public string AboutText
		{
			get { return _aboutText; }
			set
			{
				if (_aboutText != value)
				{
					_aboutText = value;
					this.RaisePropertyChanged(() => this.AboutText);
				}
			}
		}
	}
}