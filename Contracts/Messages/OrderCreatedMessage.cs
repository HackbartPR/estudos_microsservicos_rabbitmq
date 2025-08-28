using static Contracts.Messages.OrderCreatedMessage;

namespace Contracts.Messages
{
	public sealed record OrderCreatedMessage(Guid OrderId, decimal Amount, CustomerRecord Customer)
	{
		public sealed record CustomerRecord(Guid Id);
	}
}
