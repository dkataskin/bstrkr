using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using RestSharp;
using RestSharp.Deserializers;

namespace bstrkr.grabber
{
	class MainClass
	{
		private static string _request = "http://bus13.ru/php/getVehiclesMarkers.php?rids=33-0,106-0,23-0,24-0,71-0,72-0,14-0,107-0,45-0,46-0,51-0,52-0,61-0,62-0,12-0,13-0,10-0,11-0,47-0,48-0,2-0,108-0,29-0,81-0,4-0,109-0,49-0,50-0,27-0,28-0,67-0,68-0,18-0,19-0,55-0,56-0,16-0,17-0,20-0,110-0,114-0,115-0,88-0,89-0,53-0,54-0,6-0,7-0,37-0,38-0,31-0,32-0,92-0,93-0,94-0,95-0,25-0,26-0,98-0,99-0,86-0,87-0,39-0,40-0,96-0,97-0,112-0,113-0,75-0,76-0,59-0,60-0,78-0,79-0,41-0,42-0,43-0,44-0,102-0,103-0,100-0,101-0,82-0,83-0,80-0,111-0,69-0,70-0,84-0,85-0,104-0,105-0,90-0,91-0,57-0,58-0&lat0=0&lng0=0&lat1=90&lng1=180&curk={0}&city=saransk&info=01234&_={1}";
		private static int _timestamp = 0;
		private static Random _rnd = new Random();

		public static void Main(string[] args)
		{
			var delay = TimeSpan.FromSeconds(10);
			var vehicleId = "47001093";

			var cancellationTokenSource = new CancellationTokenSource();
			var token = cancellationTokenSource.Token;
			Task.Factory.StartNew(() => Update(delay, vehicleId, token), cancellationTokenSource.Token);

			Console.ReadKey();
			Console.WriteLine("Terminating...");

			cancellationTokenSource.Cancel();
		}

		private static void Update(TimeSpan sleepInterval, string vehicleId, CancellationToken token)
		{
			var restClient = new RestClient();
			restClient.RemoveHandler("text/html");
			restClient.AddHandler("text/html", new JsonDeserializer());

			while(!token.IsCancellationRequested)
			{
				try
				{
					var now = DateTime.UtcNow;
					var request = new RestRequest(string.Format(_request, _timestamp, _rnd.Next()));
					var response = restClient.Execute<VehicleMarkersResponse>(request);
					_timestamp = response.Data.maxk;

					var vehicle = response.Data.anims.FirstOrDefault(x => x.id.Equals(vehicleId));

					if (vehicle == null)
					{

						Console.WriteLine(
							"| {0} | {1} | {2:HH:mm:ss} |         |                   |", 
							_timestamp, 
							vehicleId, 
							now);
					}
					else
					{
						var points = vehicle.anim_points
							.Select(x => string.Format("{0:F2}:{1},{2}", int.Parse(x.percent) / 100.0f, x.lat, x.lon));

						Console.WriteLine(
							"| {0} | {1} | {2:HH:mm:ss} | {3} | {4} {5} | {6}", 
							_timestamp, 
							vehicleId, 
							now,
							vehicle.lasttime.Substring(11),
							vehicle.lat.ToString(),
							vehicle.lon.ToString(),
							string.Join(";", points));
					}
				} 
				catch (Exception ex)
				{
					Console.WriteLine("An error occured: {0}", ex);
				}

				Task.Delay(sleepInterval, token).Wait(token);
			}
		}
	}
}