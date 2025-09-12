using StoreApi.Infra.Models;

namespace StoreApi.Infra.DTOs {
    public class CreateSubscriptionDto {
        public int UserId { get; set; }
        public SubscriptionType Type { get; set; }
    }
}