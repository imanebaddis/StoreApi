using System.Collections.Generic;
using StoreApi.Infra.DTOs;
using StoreApi.Infra.Models;

namespace StoreApi.Infra.Services {
    public interface ISubscriptionService {
        SubscriptionResponseDto CreateSubscription(CreateSubscriptionDto dto);
        SubscriptionResponseDto GetActiveSubscription(int userId);
        IEnumerable<SubscriptionResponseDto> GetUserSubscriptions(int userId);
        void CancelSubscription(int subscriptionId);
        IEnumerable<SubscriptionPlanDto> GetAvailablePlans();
        bool CanAccessBook(int userId, int bookId);
    }
}