using System;

using Cirrious.MvvmCross.Binding.Bindings.Target;

using bstrkr.mvvm.views;

namespace bstrkr.ios.views
{
	public class MapViewZoomTargetBinding : MvxConvertingTargetBinding
	{
		public MapViewZoomTargetBinding(IMapView target) : base(target)
		{
		}

		protected IMapView MapView
		{
			get { return (IMapView)this.Target; }
		}

		public override void SubscribeToEvents()
		{
			this.MapView.ZoomChanged += this.OnZoomChanged;
		}

		private void OnZoomChanged(object sender, EventArgs args)
		{
			if (this.MapView == null)
			{
				return;
			}

			this.FireValueChanged(this.MapView.Zoom);
		}
	}
}