using System;

using Cirrious.MvvmCross.Droid.Views;

namespace bstrkr.core.android.presenters
{
    public interface ICustomPresenter : IMvxAndroidViewPresenter
    {
        void Register(Type viewModelType, IFragmentHost host);
    }
}