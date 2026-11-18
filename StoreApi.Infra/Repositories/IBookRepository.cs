using System.Collections.Generic;
using StoreApi.Infra.Models;
namespace StoreApi.Infra.Repositories {
    public interface IBookRepository {
        /// <param name="categoryId">Se valorizzato restituisce solo i libri di quella categoria.</param>
        IEnumerable<Book> GetAll(int? categoryId = null);
        Book? GetById(int id);
        Book Add(Book book);
        void Update(Book book);
        void Delete(int id);
    }
}
