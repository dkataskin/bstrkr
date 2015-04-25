using System;

using CommandLine;

namespace bustracker.cli
{
	public class Options
	{
		[Option('e', "endpoint", Required = true, HelpText = "Endpoint required.")]
		public string ServiceEndpoint { get; set; }

		[Option('v', "vehicleid", Required = false, HelpText = "Vehicle id")]
		public string VehicleId { get; set; }

		[Option('o', "outputdir", Required = false, HelpText = "Vehicle trace output directory")]
		public string OutputDir { get; set; }

		[Option('t', "trace", Required = false, HelpText = "Trace vehicle's path")]
		public bool Trace { get; set; }

		[Option("list", Required = false, HelpText = "List vehicles")]
		public bool List { get; set; }

		[Option('l', "locate", Required = false, HelpText = "Locate vehicle")]
		public bool Locate { get; set; }
	}
}