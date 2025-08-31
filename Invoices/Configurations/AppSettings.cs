namespace Invoices.Configurations
{
	public static class AppSettings
	{
		public class Database
		{
			public const string Identifier = "EnviromentVariables:Database";

			public string ConnectionString { get; set; } = string.Empty;
		}

		public class Broker
		{
			public const string Identifier = "EnviromentVariables:Broker";

			public string ConnectionString { get; set; } = string.Empty;
		}
	}
}
