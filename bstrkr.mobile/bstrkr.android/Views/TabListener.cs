using System;

using Android.App;

using Cirrious.MvvmCross.Droid.FullFragging.Fragments;

namespace bstrkr.android.views
{
	public class TabListener<T> : Java.Lang.Object, ActionBar.ITabListener where T : MvxFragment, new()
	{
		private readonly object _dataContext;
		private readonly Action _selectedCallback;

		private T _fragment;

		public TabListener(object dataContext, Action selectedCallback = null)
		{
			_dataContext = dataContext;
			_selectedCallback = selectedCallback;
		}

		public void OnTabReselected(ActionBar.Tab tab, FragmentTransaction ft)
		{
		}

		public void OnTabSelected(ActionBar.Tab tab, FragmentTransaction ft)
		{
			if (_fragment == null)
			{
				_fragment = new T();
				_fragment.DataContext = _dataContext;

				ft.Add(global::Android.Resource.Id.Content, _fragment, Guid.NewGuid().ToString());
			}
			else
			{
				ft.Attach(_fragment);
			}

			if (_selectedCallback != null)
			{
				_selectedCallback.Invoke();
			}
		}

		public void OnTabUnselected(ActionBar.Tab tab, FragmentTransaction ft)
		{
			if (_fragment != null)
			{
				ft.Detach(_fragment);
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && _fragment != null)
			{
				_fragment.Dispose();
			}
		}
	}
}