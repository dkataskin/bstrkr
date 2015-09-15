using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;

using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.Droid.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.core.android.views
{
	public class MvxAppCompatActivity : MvxAppCompatEventSourceActivity, IMvxAndroidView
	{
		public MvxAppCompatActivity()
		{
			BindingContext = new MvxAndroidBindingContext(this, this);
			this.AddEventListeners();
		}

		public object DataContext
		{
			get { return BindingContext.DataContext; }
			set { BindingContext.DataContext = value; }
		}

		public IMvxViewModel ViewModel
		{
			get { return DataContext as IMvxViewModel; }
			set
			{
				DataContext = value;
				OnViewModelSet();
			}
		}

		public void MvxInternalStartActivityForResult(Intent intent, int requestCode)
		{
			base.StartActivityForResult(intent, requestCode);
		}

		public IMvxBindingContext BindingContext { get; set; }

		public override void SetContentView(int layoutResId)
		{
			var view = this.BindingInflate(layoutResId, null);
			SetContentView(view);
		}

		protected virtual void OnViewModelSet()
		{
		}
	}
}
