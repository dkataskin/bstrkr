using System;

using bstrkr.mvvm.views;

using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Target;

namespace bstrkr.mvvm.bindings
{
    public class MapViewZoomTargetBinding : MvxConvertingTargetBinding
    {
        public MapViewZoomTargetBinding(IMapView target) : base(target)
        {
        }

        protected IMapView MapView => (IMapView)this.Target;

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
        }

        public override Type TargetType => typeof(float);

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWayToSource;

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