using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.Unit.Application.Services.Domain.User
{
    [TestClass]
    public class UserServiceTest
    {
        private Mock<IUserRepository> _mockUserRepository;
        private Mock<IEmailService> _mockEmailService;
        private UserService _userService;

        [TestInitialize]
        public void Setup()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockEmailService = new Mock<IEmailService>();
            _userService = new UserService(_mockUserRepository.Object, _mockEmailService.Object);
        }

        [TestMethod]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers()
        {
            // Arrange
            var expectedUsers = new List<User>
            {
                new User { Id = 1, Name = "John Doe", Email = "john@example.com" },
                new User { Id = 2, Name = "Jane Smith", Email = "jane@example.com" }
            };

            _mockUserRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedUsers);

            // Act
            var result = await _userService.GetAllUsersAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            _mockUserRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [TestMethod]
        public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
        {
            // Arrange
            var userId = 1;
            var expectedUser = new User { Id = userId, Name = "John Doe", Email = "john@example.com" };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(userId, result.Id);
            Assert.AreEqual("John Doe", result.Name);
            _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        }

        [TestMethod]
        public async Task GetUserByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var userId = 999;
            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.IsNull(result);
            _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        }

        [TestMethod]
        public async Task CreateUserAsync_WithValidRequest_ShouldCreateUserAndSendEmail()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Name = "John Doe",
                Email = "john@example.com"
            };

            var expectedUser = new User
            {
                Id = 1,
                Name = request.Name,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(expectedUser);
            _mockEmailService.Setup(s => s.SendWelcomeEmailAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var result = await _userService.CreateUserAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(request.Name, result.Name);
            Assert.AreEqual(request.Email, result.Email);
            _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
            _mockEmailService.Verify(s => s.SendWelcomeEmailAsync(request.Email), Times.Once);
        }

        [TestMethod]
        public async Task UpdateUserAsync_WithValidUser_ShouldUpdateUser()
        {
            // Arrange
            var userId = 1;
            var existingUser = new User
            {
                Id = userId,
                Name = "John Doe",
                Email = "john@example.com",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var request = new UpdateUserRequest
            {
                Name = "John Updated",
                Email = "john.updated@example.com"
            };

            var updatedUser = new User
            {
                Id = userId,
                Name = request.Name,
                Email = request.Email,
                CreatedAt = existingUser.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
            _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(updatedUser);

            // Act
            var result = await _userService.UpdateUserAsync(userId, request);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(request.Name, result.Name);
            Assert.AreEqual(request.Email, result.Email);
            Assert.IsTrue(result.UpdatedAt.HasValue);
            _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
            _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [TestMethod]
        public async Task UpdateUserAsync_WithInvalidUser_ShouldReturnNull()
        {
            // Arrange
            var userId = 999;
            var request = new UpdateUserRequest
            {
                Name = "John Updated",
                Email = "john.updated@example.com"
            };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act
            var result = await _userService.UpdateUserAsync(userId, request);

            // Assert
            Assert.IsNull(result);
            _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
            _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [TestMethod]
        public async Task DeleteUserAsync_WithValidId_ShouldDeleteUser()
        {
            // Arrange
            var userId = 1;
            _mockUserRepository.Setup(r => r.DeleteAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            Assert.IsTrue(result);
            _mockUserRepository.Verify(r => r.DeleteAsync(userId), Times.Once);
        }

        [TestMethod]
        public async Task DeleteUserAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var userId = 999;
            _mockUserRepository.Setup(r => r.DeleteAsync(userId)).ReturnsAsync(false);

            // Act
            var result = await _userService.DeleteUserAsync(userId);

            // Assert
            Assert.IsFalse(result);
            _mockUserRepository.Verify(r => r.DeleteAsync(userId), Times.Once);
        }
    }
}
