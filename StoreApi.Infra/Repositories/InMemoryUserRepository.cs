using System;
using System.Collections.Generic;
using System.Linq;
using StoreApi.Infra.Models;

namespace StoreApi.Infra.Repositories {
    public class InMemoryUserRepository : IUserRepository {
        private readonly List<User> _users = new();
        private int _nextId = 1;

        public InMemoryUserRepository()
        {
            // Seed some test data to match the bank card test data
            SeedData();
        }

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

        private void SeedData()
        {
            // Add test users that match the bank card test data
            _users.AddRange(new[]
            {
                new User
                {
                    Id = _nextId++,
                    Username = "johndoe",
                    Email = "john.doe@example.com",
                    PasswordHash = "hashedpassword123",
                    FirstName = "John",
                    LastName = "Doe",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new User
                {
                    Id = _nextId++,
                    Username = "janesmith",
                    Email = "jane.smith@example.com",
                    PasswordHash = "hashedpassword456",
                    FirstName = "Jane",
                    LastName = "Smith",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }
            });
        }
    }
}