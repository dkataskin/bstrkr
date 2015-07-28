using System;

using Refractored.Xam.Settings.Abstractions;
using Refractored.Xam.Settings;

namespace bstrkr.core
{
	public static class Settings
	{
		private const string AnimateMarkersKey = "animate_markers";
		private static readonly bool AnimateMarkersDefault = true;

		public static bool AnimateMarkers
		{
			get { return AppSettings.GetValueOrDefault<bool>(AnimateMarkersKey, AnimateMarkersDefault); }
			set { AppSettings.AddOrUpdateValue<bool>(AnimateMarkersKey, value); }
		}

		private static ISettings AppSettings
		{
			get
			{
				return CrossSettings.Current;
			}
		}
	}
}