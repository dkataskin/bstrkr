using System;

using bstrkr.core.spatial;
using bstrkr.mvvm.views;

using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Target;

namespace bstrkr.mvvm.bindings
{
	public class MapViewVisibleRegionTargetBinding : MvxConvertingTargetBinding
	{
		public MapViewVisibleRegionTargetBinding(IMapView target) : base(target)
		{
		}

		protected IMapView MapView
		{
			get { return (IMapView)this.Target; }
		}

		public override void SubscribeToEvents()
		{
			this.MapView.CameraLocationChanged += this.OnCameraLocationChanged;
		}

		private void OnCameraLocationChanged(object sender, CameraLocationChangedEventArgs args)
		{
			if (this.MapView == null)
			{
				return;
			}

			this.FireValueChanged(args.ProjectionBounds);
		}

		protected override void SetValueImpl(object target, object value)
		{
		}

		public override Type TargetType
		{
			get { return typeof(GeoRect); }
		}

		public override MvxBindingMode DefaultMode
		{
			get { return MvxBindingMode.OneWayToSource; }
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				var target = Target as IMapView;
				if (target != null)
				{
					target.CameraLocationChanged -= this.OnCameraLocationChanged;
				}
			}

			base.Dispose(isDisposing);
		}
	}
}