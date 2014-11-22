using System;
using System.Collections.Generic;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
	public class BusTrackerViewModelLocator : MvxDefaultViewModelLocator
	{
		private readonly IDictionary<Type, IMvxViewModel> _singletons = new Dictionary<Type, IMvxViewModel>
		{
			{ typeof(MapViewModel), null }
		};

		public override bool TryLoad(Type viewModelType, IDictionary<string, string> parameterValueLookup, out IMvxViewModel model)
		{
			if (_singletons.ContainsKey(viewModelType))
			{
				if (_singletons[viewModelType] == null)
				{
					var viewModel = base.TryLoad(viewModelType, parameterValueLookup, out model);
					_singletons[viewModelType] = viewModel;

					return viewModel;
				}
				else
				{
					return _singletons[viewModelType];
				}
			}

			return base.TryLoad(viewModelType, parameterValueLookup, out model);
		}
	}
}