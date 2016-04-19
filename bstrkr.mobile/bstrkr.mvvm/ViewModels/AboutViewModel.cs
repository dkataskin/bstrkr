using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
    public class AboutViewModel : BusTrackerViewModelBase
    {
        public AboutViewModel()
        {
            this.ShowLicensesCommand = new MvxCommand(() => this.ShowViewModel<LicensesViewModel>());
            this.ShowImageLicensesCommand = new MvxCommand(() => this.ShowViewModel<ImageLicensesViewModel>());
        }

        public MvxCommand ShowLicensesCommand { get; private set; }

        public MvxCommand ShowImageLicensesCommand { get; private set; }
    }
}