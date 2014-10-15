using System;
using System.Drawing;

using MonoTouch.UIKit;

using bstrkr.core.services.resources;

namespace bstrkr.core.ios.services.resources
{
	public class ResourceManager : ResourceManagerBase
	{
		protected override object GetImageResource(string path)
		{
			return UIImage.FromFile(path);
		}

		protected override object ScaleImage(object image, float ratio)
		{
			var uiImage = image as UIImage;
			return uiImage.Scale(new SizeF(
										uiImage.Size.Width * ratio, 
										uiImage.Size.Height * ratio));
		}
	}
}