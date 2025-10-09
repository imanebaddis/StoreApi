namespace StoreApi.Infra.DTOs
{
    public class PaymentRequestDto
    {
        public int SubscriptionId { get; set; }
        public int BankCardId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
    }
}
