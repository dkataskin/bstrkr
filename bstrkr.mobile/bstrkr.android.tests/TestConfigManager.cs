using System;
using System.IO;

using Android.Content.Res;

using bstrkr.core.config;

using Java.IO;

namespace bstrkr.android.tests
{
    public class TestConfigManager : ConfigManagerBase
    {
        private readonly AssetManager _assetManager;
        public TestConfigManager(AssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        protected override string ReadConfigFile()
        {
            string content;
            using (StreamReader sr = new StreamReader(_assetManager.Open("config.json")))
            {
                content = sr.ReadToEnd ();
            }

            return content;
        }
    }
}

