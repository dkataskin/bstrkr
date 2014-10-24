﻿using System;
using System.Drawing;

using MonoTouch.UIKit;

namespace bstrkr.ios.views
{
	public partial class ContentController : UIViewController
	{
		public ContentController() : base(null, null)
		{
		}

		public SidebarNavigation.SidebarController SidebarController { get; set; }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			View.BackgroundColor = UIColor.White;

			var title = new UILabel(new RectangleF(0, 50, 320, 30));
			title.Font = UIFont.SystemFontOfSize(24.0f);
			title.TextAlignment = UITextAlignment.Center;
			title.TextColor = UIColor.Blue;
			title.Text = "Sidebar Navigation";

			var body = new UILabel(new RectangleF(50, 120, 220, 100));
			body.Font = UIFont.SystemFontOfSize(12.0f);
			body.TextAlignment = UITextAlignment.Center;
			body.Lines = 0;
			body.Text = @"This is the content view controller.";

			var menuButton = new UIButton(UIButtonType.System);
			menuButton.Frame = new RectangleF(50, 250, 220, 30);
			menuButton.SetTitle("Toggle Side Menu", UIControlState.Normal);
			menuButton.TouchUpInside += (sender, e) => {
				this.SidebarController.ToggleMenu();
				//SidebarNavigation.SidebarController.ToggleMenu();
			};

			View.Add(title);
			View.Add(body);
			View.Add(menuButton);
		}
	}
}