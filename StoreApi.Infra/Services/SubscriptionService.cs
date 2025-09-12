using System;
using System.Collections.Generic;
using System.Linq;
using StoreApi.Infra.DTOs;
using StoreApi.Infra.Models;
using StoreApi.Infra.Repositories;
using StoreApi.Infra.Services;

namespace StoreApi.Infra.Services {
            public class SubscriptionService : ISubscriptionService {
        private readonly ISubscriptionRepository _subscriptionRepo;
        private readonly IUserRepository _userRepo;
        private readonly IBankCardService _bankCardService;

        public SubscriptionService(
            ISubscriptionRepository subscriptionRepo,
            IUserRepository userRepo,
            IBankCardService bankCardService) {
            _subscriptionRepo = subscriptionRepo;
            _userRepo = userRepo;
            _bankCardService = bankCardService;
        }

                public SubscriptionResponseDto CreateSubscription(CreateSubscriptionDto dto) {
            // Check if user exists
            var user = _userRepo.GetById(dto.UserId);
            if (user == null) throw new ArgumentException("User not found");

            // Check if user has a default payment card
            var defaultCard = _bankCardService.GetDefaultCard(dto.UserId);
            if (defaultCard == null) throw new ArgumentException("User must have a default payment card to create a subscription");

            // Cancel any existing active subscription
            var existingActive = _subscriptionRepo.GetActiveByUserId(dto.UserId);
            if (existingActive != null) {
                _subscriptionRepo.CancelSubscription(existingActive.Id);
            }

            // Create new subscription
            var subscription = new Subscription {
                UserId = dto.UserId,
                Type = dto.Type
            };

            var created = _subscriptionRepo.Add(subscription);

            // Process payment for the subscription
            var paymentRequest = new PaymentRequestDto {
                SubscriptionId = created.Id,
                BankCardId = defaultCard.Id,
                Amount = created.Price,
                Currency = "EUR"
            };

            _bankCardService.ProcessPayment(paymentRequest);

            return MapToResponseDto(created, user);
        }

        public SubscriptionResponseDto GetActiveSubscription(int userId) {
            var subscription = _subscriptionRepo.GetActiveByUserId(userId);
            if (subscription == null) return null;

            var user = _userRepo.GetById(userId);
            return MapToResponseDto(subscription, user);
        }

        public IEnumerable<SubscriptionResponseDto> GetUserSubscriptions(int userId) {
            var subscriptions = _subscriptionRepo.GetByUserId(userId);
            var user = _userRepo.GetById(userId);

            return subscriptions.Select(s => MapToResponseDto(s, user));
        }

        public void CancelSubscription(int subscriptionId) {
            _subscriptionRepo.CancelSubscription(subscriptionId);
        }

        public IEnumerable<SubscriptionPlanDto> GetAvailablePlans() {
            return new List<SubscriptionPlanDto> {
                new() {
                    Type = SubscriptionType.Basic,
                    Name = "Basic",
                    Price = 4.00m,
                    Description = "Access to basic books collection",
                    MaxBooksPerMonth = 10
                },
                new() {
                    Type = SubscriptionType.Silver,
                    Name = "Silver",
                    Price = 7.00m,
                    Description = "Access to extended books collection",
                    MaxBooksPerMonth = 25
                },
                new() {
                    Type = SubscriptionType.Gold,
                    Name = "Gold",
                    Price = 10.00m,
                    Description = "Access to all books collection",
                    MaxBooksPerMonth = -1 // Unlimited
                }
            };
        }

                public bool CanAccessBook(int userId, int bookId) {
            var subscription = _subscriptionRepo.GetActiveByUserId(userId);
            if (subscription == null) return false;

            // For now, all subscriptions can access all books
            // You can implement more complex logic based on book categories
            return subscription.IsActive && subscription.EndDate > DateTime.UtcNow;
        }

        public void AutoRenewSubscription(int subscriptionId) {
            _subscriptionRepo.AutoRenewSubscription(subscriptionId);
        }

        private SubscriptionResponseDto MapToResponseDto(Subscription subscription, User user) {
            var daysRemaining = subscription.EndDate.HasValue
                ? (subscription.EndDate.Value - DateTime.UtcNow).Days
                : 0;

            return new SubscriptionResponseDto {
                Id = subscription.Id,
                UserId = subscription.UserId,
                Username = user?.Username,
                Type = subscription.Type,
                TypeName = subscription.Type.ToString(),
                Price = subscription.Price,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                IsActive = subscription.IsActive,
                CreatedAt = subscription.CreatedAt,
                CancelledAt = subscription.CancelledAt,
                DaysRemaining = Math.Max(0, daysRemaining)
            };
        }
    }
}