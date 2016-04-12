using System;

using Cirrious.MvvmCross.Binding.Bindings.Target;

using bstrkr.core.spatial;
using bstrkr.mvvm.views;

namespace bstrkr.mvvm.bindings
{
    public class MapViewLocationTargetBinding : MvxConvertingTargetBinding
    {
        public MapViewLocationTargetBinding(IMapView target) : base(target)
        {
        }

        public override Type TargetType => typeof(GeoPoint);

        protected IMapView MapView => (IMapView)this.Target;

        public override void SubscribeToEvents()
        {
            this.MapView.CameraLocationChanged += this.OnCameraPositionChanged;
        }

        private void OnCameraPositionChanged(object sender, EventArgs args)
        {
            if (this.MapView == null)
            {
                return;
            }

            this.FireValueChanged(this.MapView.CameraLocation);
        }

        protected override void SetValueImpl(object target, object value)
        {
            var mapView = target as IMapView;
            mapView.SetCamera((GeoPoint)value, mapView.Zoom);
        }
    }
}