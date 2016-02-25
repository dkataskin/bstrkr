using System;

using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;

namespace bstrkr.android.util
{
	public class IconGenerator
	{
		private readonly Context _context;
		private readonly ViewGroup _container;
		private readonly TextView _textView;

		public IconGenerator(Context context) 
		{
			_context = context;

			_container = (ViewGroup) LayoutInflater.From(_context).Inflate(Resource.Layout.text_bubble, null);
			_textView = (TextView) _container.FindViewById(Resource.Id.text);

			_textView.SetTextAppearance(_context, Resource.Style.Bubble_TextAppearance_Dark);
		}

		public Bitmap MakeIcon(string text) 
		{
			if (_textView != null) 
			{
				_textView.Text = text;
			}

			return MakeIcon();
		}

		public Bitmap MakeIcon() 
		{
			int measureSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
			_container.Measure(measureSpec, measureSpec);

			int measuredWidth = _textView.MeasuredWidth;
			int measuredHeight = _textView.MeasuredHeight;

			_container.Layout(0, 0, measuredWidth, measuredHeight);

			var bubbleDrawable = _context.Resources.GetDrawable(Resource.Drawable.bubble);
			bubbleDrawable.SetBounds(0, 0, measuredWidth, measuredHeight);

			var bitmap = Bitmap.CreateBitmap(measuredWidth, measuredHeight, Bitmap.Config.Argb8888);
			var canvas = new Canvas(bitmap);

			bubbleDrawable.Draw(canvas);
			_container.Draw(canvas);

			return bitmap;
		}
	}
}