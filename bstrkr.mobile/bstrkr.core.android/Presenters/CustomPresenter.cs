using System;
using System.Collections.Generic;

using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.core.android.presenters
{
    public class CustomPresenter : MvxAndroidViewPresenter, ICustomPresenter
    {
        private readonly IDictionary<Type, IFragmentHost> _typeToHostMap = new Dictionary<Type, IFragmentHost>();

        public override void Show(MvxViewModelRequest request)
        {
            IFragmentHost host;
            if (_typeToHostMap.TryGetValue(request.ViewModelType, out host))
            {
                if (host.Show(request))
                {
                    return;
                }
            }

            base.Show(request);
        }

        public void Register(Type viewModelType, IFragmentHost host)
        {
            _typeToHostMap[viewModelType] = host;
        }
    }
}