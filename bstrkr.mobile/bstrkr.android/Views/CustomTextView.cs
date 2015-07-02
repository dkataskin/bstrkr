using System;

using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Android.Graphics;

namespace bstrkr.android.views
{
	[Register("bstrkr.android.views.CustomTextView")]
	public class CustomTextView : TextView
	{
		public CustomTextView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
		}

		public CustomTextView(Context context)
			: base(context)
		{
		}

		protected CustomTextView(IntPtr javaReference, JniHandleOwnership transfer)
			: base(javaReference, transfer)
		{
		}

		public bool IsBold
		{
			get 
			{ 
				return this.Typeface.IsBold;
			}

			set
			{
				if (value)
				{
					this.SetTypeface(null, TypefaceStyle.Bold);
				}
				else
				{
					this.SetTypeface(null, TypefaceStyle.Normal);
				}
			}
		}
	}
}

