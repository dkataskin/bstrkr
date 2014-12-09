﻿using System;

using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Localization;
using bstrkr.core.consts;

namespace bstrkr.mvvm.viewmodels
{
	public class BusTrackerViewModelBase : MvxViewModel
	{
		private long _id;
		private bool _isBusy;

		public long Id 
		{ 
			get { return _id; }
			protected set
			{
				if (_id != value)
				{
					_id = value;
					this.RaisePropertyChanged(() => this.Id);
				}
			}
		}

		public bool IsBusy
		{
			get { return _isBusy; }
			protected set 
			{
				if (_isBusy != value)
				{
					_isBusy = value;
					this.RaisePropertyChanged(() => this.IsBusy);
				}
			}
		}

		public IMvxLanguageBinder TextSource
		{
			get { return new MvxLanguageBinder(AppConsts.LocalizationGeneralNamespace, GetType().Name); }
		}
	}
}