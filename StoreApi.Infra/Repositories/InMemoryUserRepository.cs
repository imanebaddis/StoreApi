using System;
using System.Collections.Generic;
using System.Linq;
using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories {
    public class InMemoryUserRepository : IUserRepository {
        private readonly List<User> _users = new();
        private int _nextId = 1;

        public IEnumerable<User> GetAll() => _users;

        public User GetById(int id) => _users.FirstOrDefault(u => u.Id == id);

        public User GetByUsername(string username) =>
            _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        public User GetByEmail(string email) =>
            _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                public User Add(User user) {
         user.Id = _nextId++;
         user.CreatedAt = DateTime.UtcNow;
         user.IsActive = true;
         _users.Add(user);
         return user;
     }

     public User AuthenticateUser(string username, string passwordHash) {
         return _users.FirstOrDefault(u =>
             u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
             u.PasswordHash == passwordHash &&
             u.IsActive);
     }

        public void Update(User user) {
            var existing = GetById(user.Id);
            if (existing != null) {
                existing.Email = user.Email;
                existing.FirstName = user.FirstName;
                existing.LastName = user.LastName;
                existing.IsActive = user.IsActive;
            }
        }

        public void Delete(int id) {
            var user = GetById(id);
            if (user != null) _users.Remove(user);
        }

        public bool UsernameExists(string username) =>
            _users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        public bool EmailExists(string email) =>
            _users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}