namespace Orders.DTOs
{
	public class OutboxMessageDTO
	{
		public Guid Id { get; set; }
		
		public string ExchangeName { get; set; } = string.Empty;
		
		public string RoutingKey { get; set; } = string.Empty;
		
		public string Payload { get; set; } = string.Empty;
	}
}
