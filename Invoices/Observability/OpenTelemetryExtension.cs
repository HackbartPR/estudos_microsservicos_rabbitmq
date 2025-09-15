using OpenTelemetry.Trace;
using System.Diagnostics;

namespace Invoices.Observability
{
	public static class OpenTelemetryExtension
	{
		public static string ServiceName { get; }
		public static string ServiceVersion { get; }
		public static ActivitySource ActivitySource { get; }

		static OpenTelemetryExtension()
		{
			ServiceName = "Invoices API";
			ServiceVersion = typeof(OpenTelemetryExtensions).Assembly.GetName().Version!.ToString();
			ActivitySource = new ActivitySource(ServiceName, ServiceVersion);
		}
	}
}
