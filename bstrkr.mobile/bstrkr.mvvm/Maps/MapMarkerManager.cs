using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using Cirrious.CrossCore.Platform;
using Cirrious.CrossCore.WeakSubscription;

using bstrkr.mvvm.views;

namespace bstrkr.mvvm.maps
{
	public abstract class MapMarkerManager : IMapMarkerManager
	{
		private readonly IMapView _mapView;
		private readonly IDictionary<object, IMapMarker> _markers = new Dictionary<object, IMapMarker>();

		private IEnumerable _itemsSource;
		private IDisposable _subscription;

		public MapMarkerManager(IMapView mapView)
		{
			_mapView = mapView;
		}

		public virtual IEnumerable ItemsSource
		{
			get { return _itemsSource; }
			set { this.SetItemsSource(value); }
		}

		public T GetDataForMarker<T>(IMapMarker marker)
		{
			lock(_markers)
			{
				foreach (var keyValuePair in _markers)
				{
					if (keyValuePair.Value == marker)
					{
						return (T)keyValuePair.Key;
					}
				}

				return default(T);
			}
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
				marker.MapView = null;
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
					this.LockMarkers(() => this.AddMarkers(args.NewItems));
					break;

				case NotifyCollectionChangedAction.Remove:
					this.LockMarkers(() => this.RemoveMarkers(args.OldItems));
					break;

				case NotifyCollectionChangedAction.Replace:
					this.LockMarkers(() =>
					{
						this.RemoveMarkers(args.OldItems);
						this.AddMarkers(args.NewItems);
					});
					break;

				case NotifyCollectionChangedAction.Move:
					break;

				case NotifyCollectionChangedAction.Reset:
					this.LockMarkers(() => this.ReloadAllMarkers());
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		protected abstract IMapMarker CreateMarker(object item);

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
			_mapView.RemoveMarker(marker);

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

			_mapView.AddMarker(marker);
		}

		private void LockMarkers(Action action)
		{
			lock(_markers)
			{
				action.Invoke();
			}
		}
	}
}