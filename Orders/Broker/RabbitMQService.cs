using Microsoft.Extensions.Options;
using Orders.Configurations;
using RabbitMQ.Client;

namespace Orders.Broker
{
	public class RabbitMQService : IDisposable
	{
		private IConnection? _connection;
		private readonly AppSettings.Broker _settings;

		public RabbitMQService(IOptions<AppSettings.Broker> options) 
			=> _settings = options.Value ?? throw new ArgumentNullException("Broker: Connection String não foi encontrada.");

		public async Task<IConnection> GetConnectionAsync()
		{
			if (_connection != null)
				return _connection;

			await SetConnectionAsync();

			return _connection!;
		}

		private async Task SetConnectionAsync()
		{
			if (_connection != null)
				return;

			ConnectionFactory factory = new();
			factory.Uri = new Uri(_settings.ConnectionString);

			_connection = await factory.CreateConnectionAsync() ?? throw new Exception("Broker: Não foi possível criar conexão.");
		}

		public void Dispose()
		{
			_connection?.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
