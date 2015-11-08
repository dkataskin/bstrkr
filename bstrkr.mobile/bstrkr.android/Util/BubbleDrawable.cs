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

using Android.Graphics.Drawables;
using Android.Content.Res;
using Android.Graphics;
using Android;

namespace bstrkr.android.util
{
	public class BubbleDrawable : Drawable
	{
		private readonly Drawable _shadow;
		private readonly Drawable _mask;
		private Color _color = Color.White;

		public BubbleDrawable(Resources res) 
		{
			_mask = res.GetDrawable(Resource.Drawable.bubble_mask);
			_shadow = res.GetDrawable(Resource.Drawable.bubble_shadow);
		}

		public void SetColor(Color color) 
		{
			_color = color;
		}

		public override void Draw(Canvas canvas) 
		{
			_mask.Draw(canvas);
			canvas.DrawColor(_color, PorterDuff.Mode.SrcIn);
			_shadow.Draw(canvas);
		}

		public override void SetAlpha(int alpha) 
		{
			throw new NotImplementedException();
		}

		public override void SetColorFilter(ColorFilter cf) 
		{
			throw new NotImplementedException();
		}

		public override int Opacity 
		{
			get { return (int)Format.Translucent; }
		}

		public override void SetBounds(int left, int top, int right, int bottom) 
		{
			_mask.SetBounds(left, top, right, bottom);
			_shadow.SetBounds(left, top, right, bottom);
		}
			
		public Rect Bounds
		{
			get { return base.Bounds; }
			set 
			{
				_mask.Bounds = value;
				_shadow.Bounds = value;
				base.Bounds = value;
			}
		}

		public override bool GetPadding(Rect padding) 
		{
			return _mask.GetPadding(padding);
		}
	}
}