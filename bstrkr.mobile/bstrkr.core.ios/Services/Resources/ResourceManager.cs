using System;
using System.Drawing;

using MonoTouch.UIKit;

using bstrkr.core.services.resources;

namespace bstrkr.core.ios.services.resources
{
	public class ResourceManager : AppResourceManagerBase
	{
		protected override object GetImageResource(string path)
		{
			return UIImage.FromFile(path);
		}
	}
}