using System;

namespace bstrkr.providers
{
	public class LiveDataProviderContext
	{
		public LiveDataProviderContext(string currentUICultureCode)
		{
			this.CurrentUICultureCode = currentUICultureCode;
		}

		public string CurrentUICultureCode { get; private set; }
	}
}