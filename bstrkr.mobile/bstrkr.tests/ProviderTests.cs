﻿using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using bstrkr.core.providers;
using bstrkr.core.providers.bus13;
using bstrkr.core.spatial;

namespace bstrkr.tests
{
	[TestFixture]
	public class ProviderTests
	{
		private IBus13RouteDataService _service;

		[TestFixtureSetUp]
		public void SetUp()
		{
			_service = new Bus13RouteDataService("http://bus13.ru/php/", "saransk");
		}

		[Test]
		public void CanGetRoutes()
		{
			var task = _service.GetRoutesAsync().ConfigureAwait(false);
			var result = task.GetAwaiter().GetResult();

			Assert.IsNotNull(result);
			foreach (var route in result)
			{
				Console.WriteLine(route);

				Assert.IsNotNull(route.Id);
				Assert.IsNotNull(route.Name);
				Assert.IsNotNull(route.Type);
				Assert.IsNotNull(route.FirstStop);
				Assert.IsNotNull(route.LastStop);
			}
		}

		[Test]
		public void CanGetStops()
		{
			var task = _service.GetStopsAsync().ConfigureAwait(false);
			var result = task.GetAwaiter().GetResult();

			Assert.IsNotNull(result);
			foreach (var stop in result)
			{
				Console.WriteLine(stop);

				Assert.IsNotNull(stop.Id);
				Assert.IsNotNull(stop.Name);
			}
		}

		[Test]
		public void CanGetRouteNodes()
		{
			var task1 = _service.GetRoutesAsync().ConfigureAwait(false);
			var routes = task1.GetAwaiter().GetResult();

			var task2 = _service.GetRouteNodesAsync(routes.First()).ConfigureAwait(false);
			var polyline = task2.GetAwaiter().GetResult();

			Assert.IsNotNull(polyline);
			Assert.IsTrue(polyline.Nodes.Any());

			foreach (var node in polyline.Nodes)
			{
				Assert.IsTrue(node.Latitude > 0);
				Assert.IsTrue(node.Longitude > 0);
			}
		}

		[Test]
		public void CanGetVehicles()
		{
			var task1 = _service.GetRoutesAsync().ConfigureAwait(false);
			var routes = task1.GetAwaiter().GetResult();

			var task2 = _service.GetVehicleLocationsAsync(routes, GeoRect.EarthWide, 0).ConfigureAwait(false);
			var response = task2.GetAwaiter().GetResult();

			Assert.IsNotNull(response);
			Assert.IsTrue(response.Timestamp > 0);
			Assert.IsTrue(response.Vehicles.Any());

			foreach (var vehicle in response.Vehicles)
			{
				Assert.IsNotNull(vehicle);
				Assert.IsNotNull(vehicle.Id);
				Assert.IsTrue(vehicle.Location.Latitude > 0);
				Assert.IsTrue(vehicle.Location.Longitude > 0);
			}
		}
	}
}