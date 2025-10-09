using System.Collections.Generic;
using StoreApi.Infra.DTOs;
using StoreApi.Infra.Models;

namespace StoreApi.Infra.Services
{
    public interface IBankCardService
    {
        /// <summary>
        /// Gets the default bank card for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The default bank card or null if not found</returns>
        BankCard? GetDefaultCard(int userId);

        /// <summary>
        /// Processes a payment request
        /// </summary>
        /// <param name="paymentRequest">The payment request details</param>
        void ProcessPayment(PaymentRequestDto paymentRequest);

        /// <summary>
        /// Gets a bank card by ID
        /// </summary>
        /// <param name="id">The bank card ID</param>
        /// <returns>The bank card or null if not found</returns>
        BankCard? GetById(int id);

        /// <summary>
        /// Gets all bank cards for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Collection of bank cards</returns>
        IEnumerable<BankCard> GetByUserId(int userId);

        /// <summary>
        /// Adds a new bank card
        /// </summary>
        /// <param name="bankCard">The bank card to add</param>
        /// <returns>The added bank card with generated ID</returns>
        BankCard Add(BankCard bankCard);

        /// <summary>
        /// Updates an existing bank card
        /// </summary>
        /// <param name="bankCard">The bank card to update</param>
        /// <returns>The updated bank card</returns>
        BankCard Update(BankCard bankCard);

        /// <summary>
        /// Deletes a bank card (soft delete)
        /// </summary>
        /// <param name="id">The bank card ID to delete</param>
        void Delete(int id);

        /// <summary>
        /// Sets a bank card as the default for a user
        /// </summary>
        /// <param name="cardId">The card ID to set as default</param>
        /// <param name="userId">The user ID</param>
        void SetAsDefault(int cardId, int userId);
    }
}
