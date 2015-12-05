using System;

using Plugin.Settings.Abstractions;
using Plugin.Settings;

namespace bstrkr.core
{
	public static class Settings
	{
		private const string AnimateMarkersKey = "animate_markers";
		private const string SelectedAreaIdKey = "area_id";

		private static readonly bool AnimateMarkersDefault = true;
		private static readonly string SelectedAreaIdDefault = string.Empty;

		public static bool AnimateMarkers
		{
			get { return AppSettings.GetValueOrDefault<bool>(AnimateMarkersKey, AnimateMarkersDefault); }
			set { AppSettings.AddOrUpdateValue<bool>(AnimateMarkersKey, value); }
		}

		public static string SelectedAreaId
		{
			get { return AppSettings.GetValueOrDefault<string>(SelectedAreaIdKey, SelectedAreaIdDefault); }
			set { AppSettings.AddOrUpdateValue<string>(SelectedAreaIdKey, value); }
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