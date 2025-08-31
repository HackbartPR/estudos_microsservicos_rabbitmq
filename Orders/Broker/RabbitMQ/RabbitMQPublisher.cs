using RabbitMQ.Client;
using System.Text;

namespace Orders.Broker.RabbitMQ
{
	public class RabbitMQPublisher : IBroker
	{
		private IChannel? _channel;
		private readonly RabbitMQService _connector;

		public RabbitMQPublisher(RabbitMQService connection) 
			=> _connector = connection ?? throw new ArgumentNullException("Broker: Conexão não encontrada");

		public async Task SendMessageAsync(string message)
		{
			byte[] messageBytes = Encoding.UTF8.GetBytes(message);

			await SetChannelAsync();
			await _channel!.BasicPublishAsync(_connector.Exchange, _connector.RountingKey, messageBytes);
		}

		private async Task SetChannelAsync()
		{
			if (_channel != null)
				return;

			IConnection connection = await _connector.GetConnectionAsync();
			_channel = await connection.CreateChannelAsync() ?? throw new Exception("Broker: Não foi possível criar o channel");

			await _channel.QueueDeclareAsync(_connector.RountingKey, true, false, false, null, noWait: true);
		}

		public void Dispose()
		{
			_channel?.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
