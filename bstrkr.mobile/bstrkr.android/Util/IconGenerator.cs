/*
 * Copyright 2013 Google Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;

namespace bstrkr.android.util
{
	public class IconGenerator
	{
		private readonly Context _context;

		private ViewGroup _container;
		private RotationLayout _rotationLayout;
		private TextView _textView;
		private View _contentView;

		private int _rotation;

		private float _anchorU = 0.5f;
		private float _anchorV = 1f;
		private BubbleDrawable _background;

		public IconGenerator(Context context) 
		{
			_context = context;
			_background = new BubbleDrawable(_context.Resources);
			_container = (ViewGroup) LayoutInflater.From(_context).Inflate(Resource.Layout.text_bubble, null);
			_rotationLayout = (RotationLayout) _container.GetChildAt(0);
			_contentView = _textView = (TextView) _rotationLayout.FindViewById(Resource.Id.text);
			this.SetStyle(STYLE_DEFAULT);
		}

		public const int STYLE_DEFAULT = 1;
		public const int STYLE_WHITE = 2;
		public const int STYLE_RED = 3;
		public const int STYLE_BLUE = 4;
		public const int STYLE_GREEN = 5;
		public const int STYLE_PURPLE = 6;
		public const int STYLE_ORANGE = 7;

     	// Sets the text content, then creates an icon with the current style.
		public Bitmap MakeIcon(string text) 
		{
			if (_textView != null) 
			{
				_textView.Text = text;
			}

			return MakeIcon();
		}

     	// Creates an icon with the current content and style.
     	// This method is useful if a custom view has previously been set, or if text content is not
     	// applicable.
		public Bitmap MakeIcon() 
		{
			int measureSpec = View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
			_container.Measure(measureSpec, measureSpec);

			int measuredWidth = _container.MeasuredWidth;
			int measuredHeight = _container.MeasuredHeight;

			_container.Layout(0, 0, measuredWidth, measuredHeight);

			if (_rotation == 1 || _rotation == 3) 
			{
				measuredHeight = _container.MeasuredWidth;
				measuredWidth = _container.MeasuredHeight;
			}

			var bitmap = Bitmap.CreateBitmap(measuredWidth, measuredHeight, Bitmap.Config.Argb8888);
			bitmap.EraseColor(Color.Transparent);

			var canvas = new Canvas(bitmap);

			if (_rotation == 0) 
			{
				// do nothing
			} 
			else if (_rotation == 1) 
			{
				canvas.Translate(measuredWidth, 0);
				canvas.Rotate(90);
			} 
			else if (_rotation == 2) 
			{
				canvas.Rotate(180, measuredWidth / 2, measuredHeight / 2);
			} 
			else 
			{
				canvas.Translate(0, measuredHeight);
				canvas.Rotate(270);
			}

			_container.Draw(canvas);
			return bitmap;
		}

		/// <summary>
		/// Sets the child view for the icon. If the view contains a TextView with the id "text", operations such as
		/// SetTextAppearance and MakeIcon(String) will operate upon that TextView.
		/// </summary>
		public void setContentView(View contentView) 
		{
			_rotationLayout.RemoveAllViews();
			_rotationLayout.AddView(contentView);
			_contentView = contentView;
			var view = _rotationLayout.FindViewById(Resource.Id.text);
			_textView = (view is TextView) ? (TextView) view : null;
		}

		/// <summary>
		/// Rotates the contents of the icon.
		/// </summary>
		/// <param name="degrees">degrees the amount the contents should be rotated, as a multiple of 90 degrees.</param>
		public void setContentRotation(int degrees) 
		{
			_rotationLayout.Rotation = degrees;
		}

		/// <summary>
		/// Rotates the icon.
		/// </summary>
		/// <param name="degrees">Degrees the amount the icon should be rotated, as a multiple of 90 degrees.</param>
		public void setRotation(int degrees) 
		{
			_rotation = ((degrees + 360) % 360) / 90;
		}


     	// return u coordinate of the anchor, with rotation applied.
		public float getAnchorU() 
		{
			return rotateAnchor(_anchorU, _anchorV);
		}

		// return v coordinate of the anchor, with rotation applied.
		public float getAnchorV() 
		{
			return rotateAnchor(_anchorV, _anchorU);
		}

		// Rotates the anchor around (u, v) = (0, 0).
		private float rotateAnchor(float u, float v) 
		{
			switch (_rotation) {
				case 0:
					return u;

				case 1:
					return 1 - v;

				case 2:
					return 1 - u;

				case 3:
					return v;
			}

			throw new ArgumentException();
		}

		/// <summary>
		/// Sets the text color, size, style, hint color, and highlight color from the specified TextAppearance resource.
		/// <param name="resid">The identifier of the resource.</param>
		public void SetTextAppearance(Context context, int resid) 
		{
			if (_textView != null) 
			{
				_textView.SetTextAppearance(context, resid);
			}
		}

		/// <summary>
		/// Sets the text color, size, style, hint color, and highlight color from the specified TextAppearance resource.
		/// <param name="resid">The identifier of the resource.</param>
		public void SetTextAppearance(int resid) 
		{
			SetTextAppearance(_context, resid);
		}

     	// Sets the style of the icon. The style consists of a background and text appearance.
		public void SetStyle(int style) 
		{
			SetColor(GetStyleColor(style));
			SetTextAppearance(_context, GetTextStyle(style));
		}

     	// Sets the background to the default, with a given color tint.
     	// color the color for the background tint.
		public void SetColor(Color color) 
		{
			_background.SetColor(color);
			SetBackground(_background);
		}

     	// Set the background to a given Drawable, or remove the background.
     	// background the Drawable to use as the background, or null to remove the background.
		// View#setBackgroundDrawable is compatible with pre-API level 16 (Jelly Bean).
		public void SetBackground(Drawable background) 
		{
			_container.Background = background;

			// Force setting of padding.
			// setBackgroundDrawable does not call setPadding if the background has 0 padding.
			if (background != null) 
			{
				Rect rect = new Rect();
				background.GetPadding(rect);
				_container.SetPadding(rect.Left, rect.Top, rect.Right, rect.Bottom);
			} 
			else 
			{
				_container.SetPadding(0, 0, 0, 0);
			}
		}

		/// <summary>
		/// Sets the padding of the content view. The default padding of the content view 
		/// (i.e. text view) is 5dp top/bottom and 10dp left/right.
		/// </summary>
		/// <param name="left">the left padding in pixels.</param>
		/// <param name="top">the top padding in pixels.</param>
		/// <param name="right">the right padding in pixels.</param>
		/// <param name="bottom">the bottom padding in pixels.</param>
		public void SetContentPadding(int left, int top, int right, int bottom) 
		{
			_contentView.SetPadding(left, top, right, bottom);
		}

		private static Color GetStyleColor(int style) 
		{
			switch (style) 
			{
				default:
				case STYLE_DEFAULT:
				case STYLE_WHITE:
					return Color.White;
				case STYLE_RED:
					return new Color(204, 0, 0);
				case STYLE_BLUE:
					return new Color(0, 153, 204);
				case STYLE_GREEN:
					return new Color(102, 153, 0);
				case STYLE_PURPLE:
					return new Color(153, 51, 204);
				case STYLE_ORANGE:
					return new Color(255, 136, 0);
			}
		}

		private static int GetTextStyle(int style) 
		{
			switch (style) {
				default:
				case STYLE_DEFAULT:
				case STYLE_WHITE:
					return Resource.Style.Bubble_TextAppearance_Dark;
				case STYLE_RED:
				case STYLE_BLUE:
				case STYLE_GREEN:
				case STYLE_PURPLE:
				case STYLE_ORANGE:
					return Resource.Style.Bubble_TextAppearance_Light;
			}
		}
	}
}