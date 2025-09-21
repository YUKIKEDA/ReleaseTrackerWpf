using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Tests.Integration.Infrastructure.Database
{
    [TestClass]
    public class UserRepositoryIntegrationTest
    {
        private IDbContext _dbContext;
        private UserRepository _userRepository;

        [TestInitialize]
        public void Setup()
        {
            // Setup database context for integration testing
            _dbContext = new TestDbContext();
            _userRepository = new UserRepository(_dbContext);
            
            // Initialize test database
            InitializeTestDatabase();
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test data
            CleanupTestDatabase();
        }

        [TestMethod]
        public async Task CreateUserAsync_ShouldCreateUserInDatabase()
        {
            // Arrange
            var user = new User
            {
                Name = "Integration Test User",
                Email = "integration.test@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            // Act
            var result = await _userRepository.CreateAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Id > 0);
            Assert.AreEqual(user.Name, result.Name);
            Assert.AreEqual(user.Email, result.Email);
            
            // Verify user exists in database
            var retrievedUser = await _userRepository.GetByIdAsync(result.Id);
            Assert.IsNotNull(retrievedUser);
            Assert.AreEqual(user.Name, retrievedUser.Name);
            Assert.AreEqual(user.Email, retrievedUser.Email);
        }

        [TestMethod]
        public async Task GetUserByIdAsync_WithExistingUser_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                Name = "Test User",
                Email = "test@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            var createdUser = await _userRepository.CreateAsync(user);

            // Act
            var result = await _userRepository.GetByIdAsync(createdUser.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(createdUser.Id, result.Id);
            Assert.AreEqual(user.Name, result.Name);
            Assert.AreEqual(user.Email, result.Email);
        }

        [TestMethod]
        public async Task GetUserByIdAsync_WithNonExistingUser_ShouldReturnNull()
        {
            // Arrange
            var nonExistingId = 99999;

            // Act
            var result = await _userRepository.GetByIdAsync(nonExistingId);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetUserByEmailAsync_WithExistingEmail_ShouldReturnUser()
        {
            // Arrange
            var email = "email.test@example.com";
            var user = new User
            {
                Name = "Email Test User",
                Email = email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            await _userRepository.CreateAsync(user);

            // Act
            var result = await _userRepository.GetByEmailAsync(email);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(email, result.Email);
            Assert.AreEqual(user.Name, result.Name);
        }

        [TestMethod]
        public async Task UpdateUserAsync_ShouldUpdateUserInDatabase()
        {
            // Arrange
            var user = new User
            {
                Name = "Original Name",
                Email = "original@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            var createdUser = await _userRepository.CreateAsync(user);

            // Update user data
            createdUser.Name = "Updated Name";
            createdUser.Email = "updated@example.com";
            createdUser.UpdatedAt = DateTime.UtcNow;

            // Act
            var result = await _userRepository.UpdateAsync(createdUser);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Updated Name", result.Name);
            Assert.AreEqual("updated@example.com", result.Email);
            
            // Verify changes persisted in database
            var retrievedUser = await _userRepository.GetByIdAsync(createdUser.Id);
            Assert.AreEqual("Updated Name", retrievedUser.Name);
            Assert.AreEqual("updated@example.com", retrievedUser.Email);
        }

        [TestMethod]
        public async Task DeleteUserAsync_ShouldSoftDeleteUser()
        {
            // Arrange
            var user = new User
            {
                Name = "Delete Test User",
                Email = "delete.test@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            var createdUser = await _userRepository.CreateAsync(user);

            // Act
            var result = await _userRepository.DeleteAsync(createdUser.Id);

            // Assert
            Assert.IsTrue(result);
            
            // Verify user is soft deleted (not retrievable by normal queries)
            var retrievedUser = await _userRepository.GetByIdAsync(createdUser.Id);
            Assert.IsNull(retrievedUser);
        }

        [TestMethod]
        public async Task GetAllUsersAsync_ShouldReturnOnlyActiveUsers()
        {
            // Arrange
            var activeUser1 = new User
            {
                Name = "Active User 1",
                Email = "active1@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            var activeUser2 = new User
            {
                Name = "Active User 2",
                Email = "active2@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            var deletedUser = new User
            {
                Name = "Deleted User",
                Email = "deleted@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = true
            };

            await _userRepository.CreateAsync(activeUser1);
            await _userRepository.CreateAsync(activeUser2);
            await _userRepository.CreateAsync(deletedUser);

            // Act
            var result = await _userRepository.GetAllAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(u => !u.IsDeleted));
        }

        private void InitializeTestDatabase()
        {
            // Initialize test database schema and data
            // This would typically create tables, indexes, and seed data
        }

        private void CleanupTestDatabase()
        {
            // Clean up test data and reset database state
            // This ensures tests don't interfere with each other
        }
    }
}
