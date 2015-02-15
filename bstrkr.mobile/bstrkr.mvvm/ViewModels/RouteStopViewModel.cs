using System;

using bstrkr.providers;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class RouteStopViewModel : BusTrackerViewModelBase
	{
		private readonly ILiveDataProviderFactory _liveDataProviderFactory;

		private string _routeId;
		private string _name;
		private string _description;

		public RouteStopViewModel(ILiveDataProviderFactory liveDataProvider)
		{
			_liveDataProviderFactory = liveDataProvider;
		}

		public string RouteId 
		{
			get { return _routeId; }
			private set { this.RaiseAndSetIfChanged(ref _routeId, value, () => this.RouteId); }
		}

		public string Name 
		{
			get { return _name; }
			private set { this.RaiseAndSetIfChanged(ref _name, value, () => this.Name); }
		}

		public string Description 
		{
			get { return _description; }
			private set { this.RaiseAndSetIfChanged(ref _description, value, () => this.Description); }
		}

		public void Init(string id, string name, string description)
		{
			this.RouteId = id;
			this.Name = name;
			this.Description = description;
		}

		private void Refresh()
		{
			var provider = _liveDataProviderFactory.GetCurrentProvider();
			if (provider != null)
			{

			}
		}
	}
}