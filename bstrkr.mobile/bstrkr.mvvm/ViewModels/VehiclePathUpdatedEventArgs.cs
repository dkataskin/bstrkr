using System;
using System.Collections.Generic;

using bstrkr.core;

namespace bstrkr.mvvm.viewmodels
{
	public class VehiclePathUpdatedEventArgs : EventArgs
	{
		public IList<PathSegment> PathSegments { get; set; }
	}
}