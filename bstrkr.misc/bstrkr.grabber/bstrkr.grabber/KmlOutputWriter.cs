using System;
using System.IO;

using bstrkr.core;
using bstrkr.core.spatial;

using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using bstrkr.providers.bus13.data;
using System.Linq;

namespace bstrkr.grabber
{
	public class KmlOutputWriter : IVehicleTraceOutputWriter
	{
		private static string _outputDir;
		private KmlFileWriter _kmlWriter;

		public KmlOutputWriter(string outputDir)
		{
			_outputDir = outputDir;
		}

		public void Write(Bus13VehicleLocationUpdate update)
		{
			if (_kmlWriter == null)
			{
				_kmlWriter = GetKmlWriter(update.Vehicle.Id, _outputDir);
			}

			if (update.Waypoints != null && update.Waypoints.Any())
			{
				foreach (var waypoint in update.Waypoints)
				{
					Console.WriteLine(
						"id:{0}, fr:{1:F2}, lat:{2}, lng:{3}",
						update.Vehicle.Id, 
						waypoint.Fraction,
						waypoint.Location.Position.Latitude,
						waypoint.Location.Position.Longitude);

					_kmlWriter.AddPoint(waypoint.Location.Position);
				}
			}

			_kmlWriter.AddPoint(update.Vehicle.Location.Position);
			_kmlWriter.Save();
		}

		private static KmlFileWriter GetKmlWriter(string vehicleId, string outputDir)
		{
			var document = new Document();
			document.Name = string.Format("Vehicle {0} trace", vehicleId);
			document.Description = new Description 
			{ 
				Text = string.Format(
					"Vehicle {0} trace, starts from {1}", 
					vehicleId,
					DateTime.Now.ToString("u"))
			};

			var style = new Style 
			{
				Id = "routePathStyle",
				Line = new LineStyle 
				{
					Color = Color32.Parse("7f00ffff"),
					Width = 4
				}
			};
			document.AddStyle(style);

			var placeMark = new Placemark 
			{
				Name = string.Format("Vehicle {0} path", vehicleId),
				StyleUrl = new Uri("#routePathStyle", UriKind.RelativeOrAbsolute)
			};

			var lineString = new LineString 
			{
				Extrude = true,
				AltitudeMode = AltitudeMode.ClampToGround,
				Coordinates = new CoordinateCollection(),
				Tessellate = true
			};

			placeMark.Geometry = lineString;
			document.AddFeature(placeMark);

			var kmlOutputDirectory = new DirectoryInfo(_outputDir).FullName;
			return new KmlFileWriter 
			{
				KmlFile = KmlFile.Create(document, false),
				Path = lineString,
				OutputFile = Path.Combine(kmlOutputDirectory, string.Format("trace-{0}.kml", vehicleId))
			};
		}

		private class KmlFileWriter
		{
			public KmlFile KmlFile { get; set; }

			public LineString Path { get; set; }

			public string OutputFile { get; set; }

			public void AddPoint(GeoPoint point)
			{
				Path.Coordinates.Add(
					new Vector(
						point.Latitude,
						point.Longitude,
						1.0));
			}

			public void Save()
			{
				try 
				{
					using(var fileStream = File.OpenWrite(this.OutputFile))
					{
						KmlFile.Save(fileStream);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(
						"An error occured while saving {0}: {1}", 
						System.IO.Path.GetFileName(this.OutputFile), 
						e.ToString());
				}
			}
		}
	}
}