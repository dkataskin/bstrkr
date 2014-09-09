using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm.viewmodels
{
    public class MainViewModel : MvxViewModel
    {
		private string _hello = "Hello MvvmCross";

        public string Hello
		{ 
			get 
			{ 
				return this._hello; 
			}

			set 
			{ 
				this._hello = value; 
				this.RaisePropertyChanged(() => this.Hello); 
			}
		}
    }
}