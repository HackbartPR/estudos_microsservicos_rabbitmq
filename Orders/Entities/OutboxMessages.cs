using Orders.Enums;

namespace Orders.Entities
{
	public class OutboxMessages
	{
		public Guid Id { get; private set; } = Guid.NewGuid();

		public string ExchangeName { get; set; } = string.Empty;

		public string RoutingKey { get; set; } = string.Empty;

		public string Payload { get; set; } = string.Empty;

		public EStatusOutboxMessage Status { get; set; } = EStatusOutboxMessage.Pending;
		
		public DateTime? UpdatedAt { get; set; }

		public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
	}
}
