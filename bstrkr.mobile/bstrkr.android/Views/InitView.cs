using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using bstrkr.android;
using bstrkr.android.views;
using bstrkr.core.android.presenters;
using bstrkr.core.android.views;
using bstrkr.mvvm.viewmodels;

using Cirrious.CrossCore;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.FullFragging.Views;
using Cirrious.MvvmCross.ViewModels;

namespace Views
{
	[Activity(Label = "Init", 
			  ScreenOrientation = ScreenOrientation.Portrait,
			  Theme = "@style/BusTrackerTheme",
			  NoHistory = true)]
	[Register("bstrkr.android.views.InitView")]
	public class InitView : MvxActivity, IFragmentHost
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			this.SetContentView(Resource.Layout.page_init_view);

			this.RegisterForDetailsRequests();
		}

		public bool Show(MvxViewModelRequest request)
		{
			var loaderService = Mvx.Resolve<IMvxViewModelLoader>();
			if (request.ViewModelType == typeof(SetAreaViewModel))
			{
				var dialog = new SetAreaView();
				dialog.ViewModel = loaderService.LoadViewModel(request, null);
				dialog.Show(this.FragmentManager, null);

				return true;
			}

			return false;
		}

		private void RegisterForDetailsRequests()
		{
			var customPresenter = Mvx.Resolve<ICustomPresenter>();
			customPresenter.Register(typeof(SetAreaViewModel), this);
		}
	}
}