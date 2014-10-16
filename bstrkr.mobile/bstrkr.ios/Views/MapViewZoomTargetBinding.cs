using System;

using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Bindings.Target;

using bstrkr.mvvm.views;

namespace bstrkr.ios.views
{
	public class MapViewZoomTargetBinding : MvxConvertingTargetBinding
	{
		public MapViewZoomTargetBinding(MonoTouchGoogleMapsView target) : base(target)
		{
		}

		protected MonoTouchGoogleMapsView MapView
		{
			get { return (MonoTouchGoogleMapsView)this.Target; }
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
			//this.MapView.zo
			//binaryEdit.SetThat((int)value);
		}

		public override Type TargetType
		{
			get { return typeof(float); }
		}

		public override MvxBindingMode DefaultMode
		{
			get { return MvxBindingMode.TwoWay; }
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				var target = Target as MonoTouchGoogleMapsView;
				if (target != null)
				{
					target.ZoomChanged -= this.OnZoomChanged;
				}
			}

			base.Dispose(isDisposing);
		}
	}
}