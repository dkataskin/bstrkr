﻿using System;
using System.Collections.Generic;
using System.Linq;

using Cirrious.MvvmCross.Droid.Fragging.Fragments;

namespace bstrkr.android.views
{
	public class GenericFragmentPagerAdaptor : Android.Support.V4.App.FragmentPagerAdapter
	{
		private IDictionary<string, MvxFragment> _fragments = new Dictionary<string, MvxFragment>();

		public GenericFragmentPagerAdaptor(Android.Support.V4.App.FragmentManager fragmentManager)
			: base(fragmentManager)
		{
		}

		public override int Count
		{
			get { return _fragments.Keys.Count; }
		}

		public override Android.Support.V4.App.Fragment GetItem(int position)
		{
			return _fragments.Values.ElementAt(position);
		}

		public void AddFragment(string title, MvxFragment fragment)
		{
			_fragments.Add(title, fragment);
			NotifyDataSetChanged();
		}

		public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
		{
			return new Java.Lang.String(_fragments.Keys.ElementAt(position));
		}
	}
}