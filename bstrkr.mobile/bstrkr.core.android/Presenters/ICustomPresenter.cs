using System;

namespace bstrkr.core.android.presenters
{
	public interface ICustomPresenter
	{
		void Register(Type viewModelType, IFragmentHost host);
	}
}