using Orders.Enums;

namespace Orders.Entities
{
	public class Order
	{
		public Guid Id { get; set; } = Guid.NewGuid();

		public Guid CustomerId { get; set; }

		public decimal Amount { get; set; }

		public EStatus Status { get; set; } = EStatus.Pending;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}
