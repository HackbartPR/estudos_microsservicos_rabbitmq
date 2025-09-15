using OpenTelemetry.Trace;
using System.Diagnostics;

namespace Orders.Observability
{
	public static class OpenTelemetryExtension
	{
		public static string ServiceName { get; }
		public static string ServiceVersion { get; }
		public static ActivitySource ActivitySource { get; }

		static OpenTelemetryExtension()
		{
			ServiceName = "Orders API";
			ServiceVersion = typeof(OpenTelemetryExtensions).Assembly.GetName().Version!.ToString();
			ActivitySource = new ActivitySource(ServiceName, ServiceVersion);
		}
	}
}
