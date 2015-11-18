using System;

using Android.Runtime;
using Android.Widget;
using Android.Content;
using Android.Util;
using Android.Graphics;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.CustomImageView")]
	public class CustomImageView : ImageView
	{
		public CustomImageView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
		}

		public CustomImageView(Context context)
			: base(context)
		{
		}

		protected CustomImageView(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
		}

		public Color TintColor 
		{
			get { return new Color(); }
			set 
			{
				this.SetColorFilter(value);
			}
		}
	}
}