using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.ViewModels;

using bstrkr.mvvm.viewmodels;

namespace bstrkr.mvvm
{
    public class BusTrackerApp : MvxApplication
    {
        public override void Initialize()
        {
            this.RegisterAppStart<HomeViewModel>();
        }

		protected override IMvxViewModelLocator CreateDefaultViewModelLocator()
		{
			return new BusTrackerViewModelLocator();
		}
    }
}