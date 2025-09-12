namespace StoreApi.Infra.Models {
    public class SubscriptionPrice {
        public SubscriptionType Type { get; set; }
        public decimal Price { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}