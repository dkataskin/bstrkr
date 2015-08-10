using System;

using Cirrious.MvvmCross.Plugins.Messenger;

namespace bstrkr.mvvm.messages
{
	public class MainViewportChangedMessage : MvxMessage
	{
		public MainViewportChangedMessage(float visibleAreaOffset)
		{
			this.VisibleAreaOffset = visibleAreaOffset;
		}

		public float VisibleAreaOffset { get; set; }
	}
}