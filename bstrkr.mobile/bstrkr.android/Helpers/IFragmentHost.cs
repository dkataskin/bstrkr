using System;
using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.android.helpers
{
	public interface IFragmentHost
	{
		bool Show(MvxViewModelRequest request);
	}
}