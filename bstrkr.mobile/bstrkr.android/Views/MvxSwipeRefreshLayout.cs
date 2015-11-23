using System;
using System.Windows.Input;

using Android.Content;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Runtime;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.MvxSwipeRefreshLayout")]
	public class MvxSwipeRefreshLayout : SwipeRefreshLayout
	{
		/// <summary>
		/// Gets or sets the refresh command.
		/// </summary>
		/// <value>The refresh command.</value>
		public ICommand RefreshCommand { get; set;}

		public MvxSwipeRefreshLayout(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			this.Init();
			this.SetColorSchemeColors(
					Resource.Color.progressbar_color_1,
					Resource.Color.progressbar_color_2,
					Resource.Color.progressbar_color_3,
					Resource.Color.progressbar_color_4);
		}

		public MvxSwipeRefreshLayout(Context context)
			: base(context)
		{
			this.Init();

			this.SetColorSchemeColors(
				Resource.Color.progressbar_color_1,
				Resource.Color.progressbar_color_2,
				Resource.Color.progressbar_color_3,
				Resource.Color.progressbar_color_4);
		}

		private void Init()
		{
			// This gets called when we pull down to refresh to trigger command
			this.Refresh += (object sender, EventArgs e) => 
			{
				var command = this.RefreshCommand;
				if (command == null)
				{ 
					return;
				}

				command.Execute (null);
			};
		}
	}
}
