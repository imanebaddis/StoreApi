using StoreApi.Infra.Models;

namespace StoreApi.Infra.DTOs {
    public class SubscriptionPlanDto {
        public SubscriptionType Type { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int MaxBooksPerMonth { get; set; }
    }
}