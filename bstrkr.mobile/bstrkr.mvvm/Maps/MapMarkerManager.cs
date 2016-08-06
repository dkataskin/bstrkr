using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using bstrkr.mvvm.views;

using Cirrious.CrossCore.Platform;

namespace bstrkr.mvvm.maps
{
    public abstract class MapMarkerManager : IMapMarkerManager
    {
        private readonly IMapView _mapView;
        private readonly IDictionary<object, IMapMarker> _markers = new Dictionary<object, IMapMarker>();

        public MapMarkerManager(IMapView mapView)
        {
            _mapView = mapView;
        }

        public T GetDataForMarker<T>(IMapMarker marker)
        {
            IMapMarker result;
            _markers.TryGetValue(marker, out result);

            if (result != null)
            {
                return (T)result;
            }

            return default(T);
        }

        protected abstract IMapMarker CreateMarker(object item);

        protected void RemoveAllMarkers()
        {
            this.RemoveMarkers(_markers.Keys.ToList());
        }

        protected virtual void RemoveMarkers(IEnumerable itemsToRemove)
        {
            foreach (var item in itemsToRemove)
            {
                this.RemoveMarkerFor(item);
            }
        }

        protected virtual void RemoveMarkerFor(object item)
        {
            try
            {
                IMapMarker marker;
                if (_markers.TryGetValue(item, out marker))
                {
                    _markers.Remove(item);
                    _mapView.RemoveMarker(marker);
                }

                (marker as ICleanable)?.CleanUp();
            }
            catch (Exception ex)
            {
                MvxTrace.Warning("An error occurred while removing marker: {0}", ex);
            }
        }

        protected virtual void AddMarkers(IEnumerable itemsToAdd)
        {
            foreach (object item in itemsToAdd)
            {
                this.AddMarkerFor(item);
            }
        }

        protected virtual void AddMarkerFor(object item)
        {
            if (!_markers.ContainsKey(item))
            {
                _markers[item] = this.CreateMarker(item);
            }

            //_markers.AddOrUpdate(
            //                item,
            //                markerSource =>
            //                {
            //                    var marker = this.CreateMarker(item);
            //                    _mapView.AddMarker(marker);
            //                    return marker;
            //                }, 
            //                (markerSource, marker) => marker);

            
        }
    }
}