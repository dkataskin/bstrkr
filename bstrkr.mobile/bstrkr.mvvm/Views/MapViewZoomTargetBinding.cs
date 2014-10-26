using System;

using bstrkr.core.spatial;

using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Target;

namespace bstrkr.mvvm.views
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

		protected override void SetValueImpl(object target, object value)
		{
			var mapView = target as IMapView;
			mapView.SetCamera((GeoPoint)value, mapView.Zoom);
		}

		public override Type TargetType
		{
			get { return typeof(GeoPoint); }
		}

		public override MvxBindingMode DefaultMode
		{
			get { return MvxBindingMode.TwoWay; }
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				var target = Target as IMapView;
				if (target != null)
				{
					target.ZoomChanged -= this.OnZoomChanged;
				}
			}

			base.Dispose(isDisposing);
		}
	}
}