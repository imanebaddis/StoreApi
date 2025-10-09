using System;
using System.Collections.Generic;
using System.Linq;
using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories {
    public class InMemorySubscriptionRepository : ISubscriptionRepository {
        private readonly List<Subscription> _subscriptions = new();
        private int _nextId = 1;

        private readonly Dictionary<SubscriptionType, decimal> _prices = new() {
            { SubscriptionType.Basic, 4.00m },
            { SubscriptionType.Silver, 7.00m },
            { SubscriptionType.Gold, 10.00m }
        };

        public IEnumerable<Subscription> GetAll() => _subscriptions;

        public Subscription GetById(int id) => _subscriptions.FirstOrDefault(s => s.Id == id);

        public Subscription GetActiveByUserId(int userId) =>
            _subscriptions.FirstOrDefault(s => s.UserId == userId && s.IsActive &&
                (s.EndDate == null || s.EndDate > DateTime.UtcNow));

        public IEnumerable<Subscription> GetByUserId(int userId) =>
            _subscriptions.Where(s => s.UserId == userId);

        public Subscription Add(Subscription subscription) {
            subscription.Id = _nextId++;
            subscription.StartDate = DateTime.UtcNow;
            subscription.EndDate = subscription.StartDate.AddMonths(1);
            subscription.Price = _prices[subscription.Type];
            subscription.IsActive = true;
            subscription.CreatedAt = DateTime.UtcNow;

            _subscriptions.Add(subscription);
            return subscription;
        }

        public void Update(Subscription subscription) {
            var existing = GetById(subscription.Id);
            if (existing != null) {
                existing.Type = subscription.Type;
                existing.Price = subscription.Price;
                existing.StartDate = subscription.StartDate;
                existing.EndDate = subscription.EndDate;
                existing.IsActive = subscription.IsActive;
                existing.CancelledAt = subscription.CancelledAt;
            }
        }

        public void Delete(int id) {
            var subscription = GetById(id);
            if (subscription != null) _subscriptions.Remove(subscription);
        }

        public bool HasActiveSubscription(int userId) {
            var active = GetActiveByUserId(userId);
            return active != null;
        }

        public void CancelSubscription(int subscriptionId) {
            var subscription = GetById(subscriptionId);
            if (subscription != null) {
                subscription.IsActive = false;
                subscription.CancelledAt = DateTime.UtcNow;
            }
        }

        public IEnumerable<Subscription> GetExpiredSubscriptions() =>
            _subscriptions.Where(s => s.IsActive && s.EndDate < DateTime.UtcNow);

        public void AutoRenewSubscription(int subscriptionId) {
            var subscription = GetById(subscriptionId);
            if (subscription != null && subscription.IsActive) {
                // Auto-renew by extending the end date by one month
                subscription.EndDate = subscription.EndDate?.AddMonths(1) ?? DateTime.UtcNow.AddMonths(1);
            }
        }

        public void RenewSubscription(int subscriptionId) {
            var subscription = GetById(subscriptionId);
            if (subscription != null) {
                // Renew subscription by extending the end date by one month
                subscription.EndDate = subscription.EndDate?.AddMonths(1) ?? DateTime.UtcNow.AddMonths(1);
                subscription.IsActive = true;
            }
        }
    }
}