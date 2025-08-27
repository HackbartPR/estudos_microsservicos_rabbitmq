using Microsoft.Extensions.Options;
using Npgsql;
using Orders.Configurations;
using System.Data;

namespace Orders.Database
{
	public class DapperContext : IDisposable
	{
		public IDbConnection Connection { get; private set; }

		private readonly AppSettings.Database _configuration;

		public DapperContext(IOptions<AppSettings.Database> options)
		{
			_configuration = options.Value ?? throw new ArgumentNullException("Database: Connection String não foi encontrada.");
			Connection = new NpgsqlConnection(_configuration.ConnectionString);
		}

		public void Dispose()
		{
			Connection?.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
