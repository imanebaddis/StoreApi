using System.Collections.Generic;
using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories {
            public interface IUserRepository {
         IEnumerable<User> GetAll();
         User GetById(int id);
         User GetByUsername(string username);
         User GetByEmail(string email);
         User Add(User user);
         void Update(User user);
         void Delete(int id);
         bool UsernameExists(string username);
         bool EmailExists(string email);
         User AuthenticateUser(string username, string passwordHash);
     }
}