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

		public override IMvxViewModel Load(Type viewModelType, IMvxBundle parameterValues, IMvxBundle savedState)
		{
			if (_singletons.ContainsKey(viewModelType))
			{
				if (_singletons[viewModelType] == null)
				{
					var viewModel = base.Load(viewModelType, parameterValues, savedState);
					if (viewModel != null)
					{
						_singletons[viewModelType] = viewModel;
					}

					return viewModel;
				}
				else
				{
					return _singletons[viewModelType];
				}
			}

			return base.Load(viewModelType, parameterValues, savedState);
		}

//		public override bool TryLoad(Type viewModelType, IMvxBundle parameterValues, IMvxBundle savedState, out IMvxViewModel viewModel)
//		{
//			if (_singletons.ContainsKey(viewModelType))
//			{
//				if (_singletons[viewModelType] == null)
//				{
//					var success = base.TryLoad(viewModelType, parameterValues, savedState, out viewModel);
//					if (success)
//					{
//						_singletons[viewModelType] = viewModel;
//					}
//
//					return success;
//				}
//				else
//				{
//					viewModel = _singletons[viewModelType];
//					return true;
//				}
//			}
//
//			return base.TryLoad(viewModelType, parameterValues, savedState, out viewModel);
//		}
	}
}