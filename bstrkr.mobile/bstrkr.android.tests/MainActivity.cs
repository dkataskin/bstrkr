using System.Reflection;

using Android.App;
using Android.OS;
using Xamarin.Android.NUnitLite;
using bstrkr.core.config;

namespace bstrkr.android.tests
{
    [Activity(Label = "bstrkr.android.tests", MainLauncher = true)]
    public class MainActivity : TestSuiteActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            this.SetConfigManager();

            // tests can be inside the main assembly
            AddTest(Assembly.GetExecutingAssembly());
            // or in any reference assemblies
            // AddTest (typeof (Your.Library.TestClass).Assembly);

            // Once you called base.OnCreate(), you cannot add more assemblies.
            base.OnCreate(bundle);
        }

        private void SetConfigManager()
        {
            ConfigManager = new TestConfigManager(this.Assets);
        }

        public static IConfigManager ConfigManager { get; set; }
    }
}

