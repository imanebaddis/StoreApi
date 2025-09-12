using System;

namespace StoreApi.Infra.Models {
    public class Subscription {
        public int Id { get; set; }
        public int UserId { get; set; }
        public SubscriptionType Type { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
                    public DateTime? CancelledAt { get; set; }
    }
}