using System;
using System.Collections.Generic;
using System.Linq;
using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories
{
    public class InMemoryBankCardRepository : IBankCardRepository
    {
        private readonly List<BankCard> _bankCards = new();
        private int _nextId = 1;

        public InMemoryBankCardRepository()
        {
            // Seed some test data
            SeedData();
        }

        public BankCard? GetById(int id)
        {
            return _bankCards.FirstOrDefault(c => c.Id == id && c.IsActive);
        }

        public BankCard? GetDefaultByUserId(int userId)
        {
            return _bankCards.FirstOrDefault(c => c.UserId == userId && c.IsDefault && c.IsActive);
        }

        public IEnumerable<BankCard> GetByUserId(int userId)
        {
            return _bankCards.Where(c => c.UserId == userId && c.IsActive).ToList();
        }

        public BankCard Add(BankCard bankCard)
        {
            bankCard.Id = _nextId++;
            bankCard.CreatedAt = DateTime.UtcNow;
            
            // If this is the first card for the user, make it default
            if (!_bankCards.Any(c => c.UserId == bankCard.UserId && c.IsActive))
            {
                bankCard.IsDefault = true;
            }
            
            // If setting as default, unset other default cards for this user
            if (bankCard.IsDefault)
            {
                var existingDefaults = _bankCards.Where(c => c.UserId == bankCard.UserId && c.IsDefault && c.IsActive);
                foreach (var card in existingDefaults)
                {
                    card.IsDefault = false;
                    card.UpdatedAt = DateTime.UtcNow;
                }
            }

            _bankCards.Add(bankCard);
            return bankCard;
        }

        public BankCard Update(BankCard bankCard)
        {
            var existing = _bankCards.FirstOrDefault(c => c.Id == bankCard.Id);
            if (existing == null)
                throw new ArgumentException("Bank card not found");

            existing.CardHolderName = bankCard.CardHolderName;
            existing.ExpiryMonth = bankCard.ExpiryMonth;
            existing.ExpiryYear = bankCard.ExpiryYear;
            existing.IsDefault = bankCard.IsDefault;
            existing.IsActive = bankCard.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            // If setting as default, unset other default cards for this user
            if (bankCard.IsDefault)
            {
                var existingDefaults = _bankCards.Where(c => c.UserId == existing.UserId && c.Id != existing.Id && c.IsDefault && c.IsActive);
                foreach (var card in existingDefaults)
                {
                    card.IsDefault = false;
                    card.UpdatedAt = DateTime.UtcNow;
                }
            }

            return existing;
        }

        public void Delete(int id)
        {
            var bankCard = _bankCards.FirstOrDefault(c => c.Id == id);
            if (bankCard != null)
            {
                bankCard.IsActive = false;
                bankCard.UpdatedAt = DateTime.UtcNow;
            }
        }

        public void SetAsDefault(int cardId, int userId)
        {
            // Unset all default cards for this user
            var userCards = _bankCards.Where(c => c.UserId == userId && c.IsActive);
            foreach (var card in userCards)
            {
                card.IsDefault = card.Id == cardId;
                card.UpdatedAt = DateTime.UtcNow;
            }
        }

        private void SeedData()
        {
            // Add some test bank cards
            _bankCards.AddRange(new[]
            {
                new BankCard
                {
                    Id = _nextId++,
                    UserId = 1,
                    CardNumber = "**** **** **** 1234",
                    CardHolderName = "John Doe",
                    ExpiryMonth = "12",
                    ExpiryYear = "2025",
                    CVV = "***",
                    IsDefault = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new BankCard
                {
                    Id = _nextId++,
                    UserId = 2,
                    CardNumber = "**** **** **** 5678",
                    CardHolderName = "Jane Smith",
                    ExpiryMonth = "06",
                    ExpiryYear = "2026",
                    CVV = "***",
                    IsDefault = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }
            });
        }
    }
}
