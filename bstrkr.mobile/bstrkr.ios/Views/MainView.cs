using System.Drawing;

using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Touch.Views;

using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

using bstrkr.mvvm.viewmodels;

namespace bstrkr.ios.Views
{
    [Register("MainView")]
    public class MainView : MvxViewController
    {
        public override void ViewDidLoad()
        {
            this.View = new UIView(){ BackgroundColor = UIColor.White };
            base.ViewDidLoad();

			// ios7 layout
            if (RespondsToSelector(new Selector("edgesForExtendedLayout")))
			{
				this.EdgesForExtendedLayout = UIRectEdge.None;
			}
			   
            var label = new UILabel(new RectangleF(10, 10, 300, 40));
            this.Add(label);

            var textField = new UITextField(new RectangleF(10, 50, 300, 40));
            Add(textField);

            var set = this.CreateBindingSet<MainView, MainViewModel>();
            set.Bind(label).To(vm => vm.Hello);
            set.Bind(textField).To(vm => vm.Hello);
            set.Apply();
        }
    }
}