using System;

using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Test.Core;
using Cirrious.MvvmCross.Views;

namespace bstrkr.mobile.tests
{
	public class ViewModelTestFixtureBase : MvxIoCSupportingTest
	{
		protected MockDispatcher MockDispatcher { get; private set; }

		protected override void AdditionalSetup() 
		{
			this.MockDispatcher = new MockDispatcher();

			Ioc.RegisterSingleton<IMvxViewDispatcher>(MockDispatcher);
			Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(MockDispatcher);
		}
	}
}