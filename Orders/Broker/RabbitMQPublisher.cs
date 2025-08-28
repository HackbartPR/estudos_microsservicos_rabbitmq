using RabbitMQ.Client;
using System.Text;

namespace Orders.Broker
{
	public class RabbitMQPublisher : IDisposable
	{
		private IChannel? _channel;
		private readonly RabbitMQService _connector;

		public RabbitMQPublisher(RabbitMQService connection) 
			=> _connector = connection ?? throw new ArgumentNullException("Broker: Conexão não encontrada");

		public async Task SendMessageAsync(string message, string exchange, string routingKey)
		{
			byte[] messageBytes = Encoding.UTF8.GetBytes(message);

			await SetChannelAsync(routingKey);
			await _channel!.BasicPublishAsync(exchange, routingKey, messageBytes);
		}

		private async Task SetChannelAsync(string queue)
		{
			if (_channel != null)
				return;

			IConnection connection = await _connector.GetConnectionAsync();
			_channel = await connection.CreateChannelAsync() ?? throw new Exception("Broker: Não foi possível criar o channel");

			await _channel.QueueDeclareAsync(queue, true, false, false, null, noWait: true);
		}

		public void Dispose()
		{
			_channel?.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
