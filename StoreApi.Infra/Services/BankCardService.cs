using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using StoreApi.Infra.DTOs;
using StoreApi.Infra.Models;
using StoreApi.Infra.Repositories;

namespace StoreApi.Infra.Services
{
    public class BankCardService : IBankCardService
    {
        private readonly IBankCardRepository _bankCardRepository;
        private readonly ILogger<BankCardService>? _logger;

        public BankCardService(IBankCardRepository bankCardRepository, ILogger<BankCardService>? logger = null)
        {
            _bankCardRepository = bankCardRepository ?? throw new ArgumentNullException(nameof(bankCardRepository));
            _logger = logger;
        }

        public BankCard? GetDefaultCard(int userId)
        {
            try
            {
                _logger?.LogInformation("Getting default card for user {UserId}", userId);
                var defaultCard = _bankCardRepository.GetDefaultByUserId(userId);
                
                if (defaultCard == null)
                {
                    _logger?.LogWarning("No default card found for user {UserId}", userId);
                }
                
                return defaultCard;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting default card for user {UserId}", userId);
                throw;
            }
        }

        public void ProcessPayment(PaymentRequestDto paymentRequest)
        {
            try
            {
                _logger?.LogInformation("Processing payment for subscription {SubscriptionId} with card {BankCardId}, amount {Amount} {Currency}", 
                    paymentRequest.SubscriptionId, paymentRequest.BankCardId, paymentRequest.Amount, paymentRequest.Currency);

                // Validate payment request
                if (paymentRequest.Amount <= 0)
                {
                    throw new ArgumentException("Payment amount must be greater than zero");
                }

                if (paymentRequest.BankCardId <= 0)
                {
                    throw new ArgumentException("Invalid bank card ID");
                }

                // Get the bank card to validate it exists and is active
                var bankCard = _bankCardRepository.GetById(paymentRequest.BankCardId);
                if (bankCard == null)
                {
                    throw new ArgumentException($"Bank card with ID {paymentRequest.BankCardId} not found");
                }

                if (!bankCard.IsActive)
                {
                    throw new ArgumentException($"Bank card with ID {paymentRequest.BankCardId} is not active");
                }

                // Validate card expiry (basic validation)
                if (!IsCardValid(bankCard))
                {
                    throw new ArgumentException($"Bank card with ID {paymentRequest.BankCardId} is expired or invalid");
                }

                // Mock payment processing - in a real implementation, this would integrate with a payment gateway
                // For now, we'll simulate a successful payment
                _logger?.LogInformation("Payment processed successfully for subscription {SubscriptionId}", paymentRequest.SubscriptionId);
                
                // In a real implementation, you might:
                // 1. Call external payment gateway API
                // 2. Store payment transaction record
                // 3. Handle payment failures and retries
                // 4. Send payment confirmation emails
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing payment for subscription {SubscriptionId}", paymentRequest.SubscriptionId);
                throw;
            }
        }

        public BankCard? GetById(int id)
        {
            try
            {
                _logger?.LogDebug("Getting bank card by ID {CardId}", id);
                return _bankCardRepository.GetById(id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting bank card by ID {CardId}", id);
                throw;
            }
        }

        public IEnumerable<BankCard> GetByUserId(int userId)
        {
            try
            {
                _logger?.LogDebug("Getting bank cards for user {UserId}", userId);
                return _bankCardRepository.GetByUserId(userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting bank cards for user {UserId}", userId);
                throw;
            }
        }

        public BankCard Add(BankCard bankCard)
        {
            try
            {
                _logger?.LogInformation("Adding new bank card for user {UserId}", bankCard.UserId);
                
                // Validate bank card data
                ValidateBankCard(bankCard);
                
                var addedCard = _bankCardRepository.Add(bankCard);
                _logger?.LogInformation("Bank card {CardId} added successfully for user {UserId}", addedCard.Id, addedCard.UserId);
                
                return addedCard;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding bank card for user {UserId}", bankCard.UserId);
                throw;
            }
        }

        public BankCard Update(BankCard bankCard)
        {
            try
            {
                _logger?.LogInformation("Updating bank card {CardId}", bankCard.Id);
                
                // Validate bank card data
                ValidateBankCard(bankCard);
                
                var updatedCard = _bankCardRepository.Update(bankCard);
                _logger?.LogInformation("Bank card {CardId} updated successfully", updatedCard.Id);
                
                return updatedCard;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating bank card {CardId}", bankCard.Id);
                throw;
            }
        }

        public void Delete(int id)
        {
            try
            {
                _logger?.LogInformation("Deleting bank card {CardId}", id);
                _bankCardRepository.Delete(id);
                _logger?.LogInformation("Bank card {CardId} deleted successfully", id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting bank card {CardId}", id);
                throw;
            }
        }

        public void SetAsDefault(int cardId, int userId)
        {
            try
            {
                _logger?.LogInformation("Setting bank card {CardId} as default for user {UserId}", cardId, userId);
                _bankCardRepository.SetAsDefault(cardId, userId);
                _logger?.LogInformation("Bank card {CardId} set as default for user {UserId}", cardId, userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error setting bank card {CardId} as default for user {UserId}", cardId, userId);
                throw;
            }
        }

        private void ValidateBankCard(BankCard bankCard)
        {
            if (bankCard == null)
                throw new ArgumentNullException(nameof(bankCard));

            if (bankCard.UserId <= 0)
                throw new ArgumentException("User ID must be greater than zero");

            if (string.IsNullOrWhiteSpace(bankCard.CardHolderName))
                throw new ArgumentException("Card holder name is required");

            if (string.IsNullOrWhiteSpace(bankCard.ExpiryMonth))
                throw new ArgumentException("Expiry month is required");

            if (string.IsNullOrWhiteSpace(bankCard.ExpiryYear))
                throw new ArgumentException("Expiry year is required");

            // Validate expiry month (01-12)
            if (!int.TryParse(bankCard.ExpiryMonth, out int month) || month < 1 || month > 12)
                throw new ArgumentException("Invalid expiry month");

            // Validate expiry year (current year or future)
            if (!int.TryParse(bankCard.ExpiryYear, out int year) || year < DateTime.Now.Year)
                throw new ArgumentException("Invalid expiry year");
        }

        private bool IsCardValid(BankCard bankCard)
        {
            if (!int.TryParse(bankCard.ExpiryMonth, out int month) || month < 1 || month > 12)
                return false;

            if (!int.TryParse(bankCard.ExpiryYear, out int year))
                return false;

            var expiryDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            return expiryDate >= DateTime.Now.Date;
        }
    }
}
