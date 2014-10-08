using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using Cirrious.CrossCore.Platform;
using Cirrious.CrossCore.WeakSubscription;
//using Cirrious.MvvmCross.Binding;
//using Cirrious.MvvmCross.Binding.Attributes;
using bstrkr.mvvm.views;

namespace bstrkr.mvvm.maps
{
	public abstract class MvxMarkerManager : IMvxMarkerManager
	{
		private readonly IMapView _mapView;
		private readonly IDictionary<object, IVehicleMarker> _markers = new Dictionary<object, IVehicleMarker>();

		private IEnumerable _itemsSource;
		private IDisposable _subscription;

		public MvxMarkerManager(IMapView mapView)
		{
			_mapView = mapView;
		}

		//[MvxSetToNullAfterBinding]
		public virtual IEnumerable ItemsSource
		{
			get { return _itemsSource; }
			set { this.SetItemsSource(value); }
		}

		protected virtual void SetItemsSource(IEnumerable value)
		{
			if (_itemsSource == value)
			{
				return;
			}

			if (_subscription != null)
			{
				_subscription.Dispose();
				_subscription = null;
			}

			_itemsSource = value;
			if (_itemsSource != null && !(_itemsSource is IList))
			{
				//MvxBindingTrace.Trace(
				//			MvxTraceLevel.Warning,
				//			"Binding to IEnumerable rather than IList - this can be inefficient, especially for large lists");
			}

			this.ReloadAllMarkers();

			var newObservable = _itemsSource as INotifyCollectionChanged;
			if (newObservable != null)
			{
				_subscription = newObservable.WeakSubscribe(OnItemsSourceCollectionChanged);
			}
		}

		protected virtual void ReloadAllMarkers()
		{
			foreach (var marker in _markers.Values) 
			{
				marker.Map = null;
			}

			_markers.Clear();

			if (_itemsSource == null)
			{
				return;
			}

			this.AddMarkers(_itemsSource);
		}

		protected virtual void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					this.AddMarkers(args.NewItems);
					break;

				case NotifyCollectionChangedAction.Remove:
					this.RemoveMarkers(args.OldItems);
					break;

				case NotifyCollectionChangedAction.Replace:
					this.RemoveMarkers(args.OldItems);
					this.AddMarkers(args.NewItems);
					break;

				case NotifyCollectionChangedAction.Move:
					break;

				case NotifyCollectionChangedAction.Reset:
					this.ReloadAllMarkers();
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		protected abstract IVehicleMarker CreateMarker(object item);

		protected virtual void RemoveMarkers(IEnumerable oldItems)
		{
			foreach (var item in oldItems)
			{
				this.RemoveMarkerFor(item);
			}
		}

		protected virtual void RemoveMarkerFor(object item)
		{
			var marker = _markers[item];

			marker.Map = null;
			_markers.Remove(item);
		}

		protected virtual void AddMarkers(IEnumerable newItems)
		{
			foreach (object item in newItems)
			{
				this.AddMarkerFor(item);
			}
		}

		protected virtual void AddMarkerFor(object item)
		{
			var marker = this.CreateMarker(item);
			_markers[item] = marker;

			marker.Map = _mapView;
		}
	}
}