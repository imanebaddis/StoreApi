using System;
using StoreApi.Infra.Models;

namespace StoreApi.Infra.DTOs {
    public class SubscriptionResponseDto {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public SubscriptionType Type { get; set; }
        public string TypeName { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public int DaysRemaining { get; set; }
    }
}