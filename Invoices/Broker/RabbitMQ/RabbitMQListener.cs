using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace Invoices.Broker.RabbitMQ
{
	public class RabbitMQListener : IBroker
	{
		private IChannel? _channel;
		private readonly RabbitMQService _connector;

		public RabbitMQListener(RabbitMQService connection)
			=> _connector = connection ?? throw new ArgumentNullException("Broker: Conexão não encontrada");

		public async Task ReceiveMessageAsync(Func<Task> callback)
		{
			await SetChannelAsync();

			var consumer = new AsyncEventingBasicConsumer(_channel!);
			consumer.ReceivedAsync += async (ch, ea) =>
			{
				var body = ea.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);

				Console.WriteLine($" [x] Received {message}");
				await callback();

				await _channel!.BasicAckAsync(ea.DeliveryTag, false);
			};

			await _channel!.BasicConsumeAsync(_connector.RountingKey, autoAck: false, consumer);
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
