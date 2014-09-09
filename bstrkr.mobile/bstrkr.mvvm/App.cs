using Cirrious.CrossCore.IoC;
using Cirrious.MvvmCross.ViewModels;

namespace bstrkr.mvvm
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            this.CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();
				
            this.RegisterAppStart<ViewModels.FirstViewModel>();
        }
    }
}