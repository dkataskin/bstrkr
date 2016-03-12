using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
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
		private const int RequestLocationId = 0;

		private readonly string [] _permissionsLocation = 
		{
			Manifest.Permission.AccessCoarseLocation,
			Manifest.Permission.AccessFineLocation
		};

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			this.SetContentView(bstrkr.android.Resource.Layout.page_init_view);

			this.RegisterForDetailsRequests();

			this.RequestPermissionsAndStart();
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

		private void RequestPermissionsAndStart()
		{
			if ((int)Build.VERSION.SdkInt < 23)
			{
				(this.ViewModel as InitViewModel).DetectLocationCommand.Execute();
				return;
			}

			this.GetLocationPermission(this.Window.DecorView.RootView);
		}

		private void GetLocationPermission(View view)
		{
			const string permission = Manifest.Permission.AccessFineLocation;
			if (this.CheckSelfPermission(permission) == (int)Permission.Granted)
			{
				(this.ViewModel as InitViewModel).DetectLocationCommand.Execute();
				return;
			}

			// need to request permission
			if (this.ShouldShowRequestPermissionRationale(permission))
			{
				// Explain to the user why we need to read the contacts
				Snackbar.Make(view, "Location access is required to show coffee shops nearby.", Snackbar.LengthIndefinite)
						.SetAction("OK", v => RequestPermissions(_permissionsLocation, RequestLocationId))
						.Show();
				
				return;
			}

			// Finally request permissions with the list of permissions and Id
			this.RequestPermissions(_permissionsLocation, RequestLocationId);
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			switch (requestCode)
			{
				case RequestLocationId:
				{
					if (grantResults[0] == Permission.Granted)
					{
						// Permission granted
						(this.ViewModel as InitViewModel).DetectLocationCommand.Execute();
					} 
					else
					{
						// Permission Denied
						(this.ViewModel as InitViewModel).SelectManuallyCommand.Execute();
					}
				}
				break;
			}
		}
	}
}