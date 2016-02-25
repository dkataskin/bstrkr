using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using System.Linq;

using Android.Gms.Maps.Model;

using bstrkr.core.android.extensions;
using bstrkr.core.spatial;
using bstrkr.mvvm.viewmodels;
using bstrkr.mvvm.views;

namespace bstrkr.android.views
{
	public abstract class GoogleMapsMarkerBase : IMapMarker
	{
		private readonly ConcurrentDictionary<string, Marker> _markers = new ConcurrentDictionary<string, Marker>();
		private readonly ReadOnlyDictionary<string, Marker> _markersReadOnly;

		public GoogleMapsMarkerBase()
		{
			_markersReadOnly = new ReadOnlyDictionary<string, Marker>(_markers);
		}

		public IReadOnlyDictionary<string, Marker> Markers { get { return _markersReadOnly; } }

		public virtual IMapView MapView { get; set; }

		public virtual GeoPoint Location 
		{
			get 
			{
				var marker = _markers.Values.FirstOrDefault();
				if (marker == null)
				{
					return GeoPoint.Empty;
				}

				return marker.Position.ToGeoPoint();
			}

			set
			{
				foreach (var marker in _markers.Values)
				{
					marker.Position = value.ToLatLng();
				}
			}
		}

		public abstract IDictionary<string, MarkerOptions> GetOptions();

		public virtual void AttachMarker(string key, Marker marker)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("Key must not be null or empty.", "key");
			}

			if (_markers.ContainsKey(key))
			{
				throw new Exception("Marker with specified key has already been attached.");
			}

			_markers.AddOrUpdate(key, marker, (key1, oldValue) => oldValue);
		}

		public virtual void DetachMarker(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("Key must not be null or empty.", "key");
			}

			if (_markers.ContainsKey(key))
			{
				throw new Exception("Marker with specified key has not been attached.");
			}

			Marker marker;
			_markers.TryRemove(key, out marker);
		}

		protected float ConvertSelectionStateToAlpha(MapMarkerSelectionStates selectionState)
		{
			switch (selectionState)
			{
				case MapMarkerSelectionStates.NoSelection:
					return 1.0f;

				case MapMarkerSelectionStates.SelectionSelected:
					return 1.0f;

				case MapMarkerSelectionStates.SelectionNotSelected:
					return 0.5f;

				default:
					return 1.0f;
			}
		}
	}
}