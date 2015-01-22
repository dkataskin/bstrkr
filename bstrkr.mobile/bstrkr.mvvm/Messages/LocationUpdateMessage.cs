using System;

using Cirrious.MvvmCross.Plugins.Messenger;

using bstrkr.core;

namespace bstrkr.mvvm.messages
{
	public class LocationUpdateMessage : MvxMessage
	{
		public LocationUpdateMessage(object sender, Area area) : base(sender)
		{
			this.Area = area;
		}

		public Area Area { get; private set; }
	}
}