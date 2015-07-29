using System;

using bstrkr.core;

using Cirrious.MvvmCross.Plugins.Messenger;

namespace bstrkr.mvvm.messages
{
	public class PreferencesChangedMessage : MvxMessage
	{
		public PreferencesChangedMessage(object sender, bool animateMarkers, Area area) : base(sender)
		{
			this.AnimateMarkers = animateMarkers;
			this.Area = area;
		}

		public bool AnimateMarkers { get; private set; }

		public Area Area { get; private set; }
	}
}