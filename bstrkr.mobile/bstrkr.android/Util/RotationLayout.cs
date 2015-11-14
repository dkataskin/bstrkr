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
using Android.Widget;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Runtime;

namespace bstrkr.android.util
{
	[Register("bstrkr.android.utils.RotationLayout")]
	public class RotationLayout : FrameLayout
	{
		private int _rotation;

		public RotationLayout(Context context) : base(context) 
		{
		}

		public RotationLayout(Context context, IAttributeSet attrs) : base(context, attrs) 
		{
		}

		public RotationLayout(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec) 
		{
			if (_rotation == 1 || _rotation == 3) 
			{
				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
				this.SetMeasuredDimension(this.MeasuredHeight, this.MeasuredWidth);
			} 
			else 
			{
				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
			}
		}

     	//degrees the rotation, in degrees.
		public void SetViewRotation(int degrees) 
		{
			_rotation = ((degrees + 360) % 360) / 90;
		}

		protected override void DispatchDraw(Canvas canvas) 
		{
			if (_rotation == 0) 
			{
				base.DispatchDraw(canvas);
				return;
			}

			if (_rotation == 1) 
			{
				canvas.Translate(this.Width, 0);
				canvas.Rotate(90, this.Width / 2, 0);
				canvas.Translate(this.Height / 2, this.Width / 2);
			} 
			else if (_rotation == 2) 
			{
				canvas.Rotate(180, this.Width / 2, this.Height / 2);
			} 
			else 
			{
				canvas.Translate(0, this.Height);
				canvas.Rotate(270, this.Width / 2, 0);
				canvas.Translate(this.Height / 2, - this.Width / 2);
			}

			base.DispatchDraw(canvas);
		}
	}
}