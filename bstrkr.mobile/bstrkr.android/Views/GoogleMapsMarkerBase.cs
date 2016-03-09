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

using Xamarin;

namespace bstrkr.android.views
{
	public abstract class GoogleMapsMarkerBase : IMapMarker
	{
		private readonly ConcurrentDictionary<string, Marker> _markers = new ConcurrentDictionary<string, Marker>();
		private readonly IList<Marker> _markersFlat = new List<Marker>();
		private readonly ReadOnlyCollection<Marker> _markersFlatReadOnly;
		private readonly ReadOnlyDictionary<string, Marker> _markersReadOnly;

		public GoogleMapsMarkerBase()
		{
			_markersReadOnly = new ReadOnlyDictionary<string, Marker>(_markers);
			_markersFlatReadOnly = new ReadOnlyCollection<Marker>(_markersFlat);
		}

		public IReadOnlyDictionary<string, Marker> Markers { get { return _markersReadOnly; } }

		public IReadOnlyCollection<Marker> MarkersFlat { get { return _markersFlatReadOnly; } }

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
			_markersFlat.Add(marker);
		}

		public virtual void DetachMarker(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("Key must not be null or empty.", "key");
			}

			if (!_markers.ContainsKey(key))
			{
				throw new Exception("Marker with specified key has not been attached.");
			}

			Marker marker;
			_markers.TryRemove(key, out marker);
			if (marker != null)
			{
				_markersFlat.Remove(marker);
			}
		}

		protected virtual void UpdateIcon(string key, BitmapDescriptor icon)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentException("Key must not be null or empty.", "key");
			}

			try
			{
				Marker marker;
				if (_markers.TryGetValue(key, out marker))
				{
					marker.SetIcon(icon);
				}	
			} 
			catch (Exception e)
			{
				Insights.Report(e, Insights.Severity.Warning);
			}
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