using System;

using Cirrious.MvvmCross.Binding.Bindings.Target;

namespace bstrkr.mvvm.views
{
	public class MapViewLocationTargetBinding : MvxConvertingTargetBinding
	{
		public MapViewLocationTargetBinding(IMapView target) : base(target)
		{
		}

		protected IMapView MapView
		{
			get { return (IMapView)this.Target; }
		}

		public override void SubscribeToEvents()
		{
			this.MapView.CameraPositionChanged += this.OnCameraPositionChanged;
		}

		private void OnCameraPositionChanged(object sender, EventArgs args)
		{
			if (this.MapView == null)
			{
				return;
			}

			this.FireValueChanged(this.MapView.CameraPosition);
		}

		protected override void SetValueImpl(object target, object value)
		{
		}
	}
}