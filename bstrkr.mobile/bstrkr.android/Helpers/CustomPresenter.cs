﻿using System;
using System.Collections.Generic;

using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.android.helpers
{
	public class CustomPresenter
		: MvxAndroidViewPresenter
	, ICustomPresenter
	{
		private Dictionary<Type, IFragmentHost> _dictionary = new Dictionary<Type, IFragmentHost>();

		public override void Show(MvxViewModelRequest request)
		{
			IFragmentHost host;
			if (this._dictionary.TryGetValue(request.ViewModelType, out host))
			{
				if (host.Show(request))
				{
					return;
				}
			}

			base.Show(request);
		}

		public void Register(Type viewModelType, IFragmentHost host)
		{
			this._dictionary[viewModelType] = host;
		}
	}
}