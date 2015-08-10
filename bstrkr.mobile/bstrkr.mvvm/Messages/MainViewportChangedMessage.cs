using System;

using Cirrious.MvvmCross.Plugins.Messenger;

namespace bstrkr.mvvm.messages
{
	public class MainViewportChangedMessage : MvxMessage
	{
		public MainViewportChangedMessage(object sender, float visibleAreaOffset) : base(sender)
		{
			this.VisibleAreaOffset = visibleAreaOffset;
		}

		public float VisibleAreaOffset { get; set; }
	}
}