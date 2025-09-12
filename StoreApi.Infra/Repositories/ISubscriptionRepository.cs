using System;
using System.Collections.Generic;
using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories {
    public interface ISubscriptionRepository {
        IEnumerable<Subscription> GetAll();
        Subscription GetById(int id);
        Subscription GetActiveByUserId(int userId);
        IEnumerable<Subscription> GetByUserId(int userId);
        Subscription Add(Subscription subscription);
        void Update(Subscription subscription);
        void Delete(int id);
        bool HasActiveSubscription(int userId);
        void CancelSubscription(int subscriptionId);
        IEnumerable<Subscription> GetExpiredSubscriptions();
        void RenewSubscription(int subscriptionId);
    }
}