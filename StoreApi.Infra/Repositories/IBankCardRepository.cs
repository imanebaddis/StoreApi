using System.Collections.Generic;
using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories
{
    public interface IBankCardRepository
    {
        BankCard? GetById(int id);
        BankCard? GetDefaultByUserId(int userId);
        IEnumerable<BankCard> GetByUserId(int userId);
        BankCard Add(BankCard bankCard);
        BankCard Update(BankCard bankCard);
        void Delete(int id);
        void SetAsDefault(int cardId, int userId);
    }
}
