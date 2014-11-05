using System;

using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.core.android.presenters
{
	public interface IFragmentHost
	{
		bool Show(MvxViewModelRequest request);
	}
}