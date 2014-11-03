using System;

namespace bstrkr.android.helpers
{
	public interface ICustomPresenter
	{
		void Register(Type viewModelType, IFragmentHost host);
	}
}