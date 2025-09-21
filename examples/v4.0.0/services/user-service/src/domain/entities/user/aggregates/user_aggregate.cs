using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Events.User.EventHandlers;

namespace Domain.Entities.User.Aggregates
{
    public class UserAggregate : AggregateRoot
    {
        private UserProfile _profile;
        private UserCredentials _credentials;
        private UserAddress _address;
        private UserPreferences _preferences;
        private readonly List<UserRole> _roles;
        private readonly List<UserSession> _sessions;

        public UserId Id { get; private set; }
        public string Email => _credentials.Email;
        public string FullName => $"{_profile.FirstName} {_profile.LastName}";
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsEmailVerified { get; private set; }
        public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();
        public IReadOnlyCollection<UserSession> Sessions => _sessions.AsReadOnly();

        private UserAggregate() 
        {
            _roles = new List<UserRole>();
            _sessions = new List<UserSession>();
        }

        public static UserAggregate Create(
            string firstName,
            string lastName,
            string email,
            string password,
            DateTime dateOfBirth,
            string phoneNumber,
            UserAddress address)
        {
            var user = new UserAggregate();
            
            user.Id = UserId.NewId();
            user._profile = new UserProfile(firstName, lastName, dateOfBirth, phoneNumber);
            user._credentials = new UserCredentials(email, password);
            user._address = address;
            user._preferences = new UserPreferences();
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
            user.IsEmailVerified = false;

            // Add default role
            user._roles.Add(UserRole.CreateDefault(user.Id));

            // Raise domain event
            user.AddDomainEvent(new UserCreatedEvent(
                user.Id.Value,
                user.Email,
                user.FullName,
                user.CreatedAt
            ));

            return user;
        }

        public void UpdateProfile(string firstName, string lastName, DateTime dateOfBirth, string phoneNumber)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot update profile of inactive user");

            var oldProfile = _profile;
            _profile = new UserProfile(firstName, lastName, dateOfBirth, phoneNumber);
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new UserProfileUpdatedEvent(
                Id.Value,
                oldProfile.FirstName,
                oldProfile.LastName,
                firstName,
                lastName,
                UpdatedAt.Value
            ));
        }

        public void UpdateAddress(UserAddress newAddress)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot update address of inactive user");

            var oldAddress = _address;
            _address = newAddress;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new UserAddressUpdatedEvent(
                Id.Value,
                oldAddress,
                newAddress,
                UpdatedAt.Value
            ));
        }

        public void VerifyEmail()
        {
            if (IsEmailVerified)
                return;

            IsEmailVerified = true;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new UserEmailVerifiedEvent(
                Id.Value,
                Email,
                UpdatedAt.Value
            ));
        }

        public void ChangePassword(string currentPassword, string newPassword)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot change password of inactive user");

            if (!_credentials.VerifyPassword(currentPassword))
                throw new InvalidOperationException("Current password is incorrect");

            var oldPasswordHash = _credentials.PasswordHash;
            _credentials = new UserCredentials(_credentials.Email, newPassword);
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new UserPasswordChangedEvent(
                Id.Value,
                oldPasswordHash,
                _credentials.PasswordHash,
                UpdatedAt.Value
            ));
        }

        public void AddRole(UserRole role)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot add role to inactive user");

            if (_roles.Any(r => r.RoleName == role.RoleName))
                return;

            _roles.Add(role);
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new UserRoleAddedEvent(
                Id.Value,
                role.RoleName,
                role.AssignedBy,
                UpdatedAt.Value
            ));
        }

        public void RemoveRole(string roleName)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot remove role from inactive user");

            var role = _roles.FirstOrDefault(r => r.RoleName == roleName);
            if (role == null)
                return;

            _roles.Remove(role);
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new UserRoleRemovedEvent(
                Id.Value,
                roleName,
                UpdatedAt.Value
            ));
        }

        public UserSession CreateSession(string ipAddress, string userAgent)
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot create session for inactive user");

            var session = new UserSession(
                sessionId: Guid.NewGuid(),
                userId: Id,
                ipAddress: ipAddress,
                userAgent: userAgent,
                createdAt: DateTime.UtcNow
            );

            _sessions.Add(session);
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new UserSessionCreatedEvent(
                Id.Value,
                session.SessionId,
                ipAddress,
                userAgent,
                UpdatedAt.Value
            ));

            return session;
        }

        public void DeactivateUser(string reason)
        {
            if (!IsActive)
                return;

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;

            // Invalidate all active sessions
            foreach (var session in _sessions.Where(s => s.IsActive))
            {
                session.Deactivate();
            }

            AddDomainEvent(new UserDeactivatedEvent(
                Id.Value,
                reason,
                UpdatedAt.Value
            ));
        }

        public void ReactivateUser()
        {
            if (IsActive)
                return;

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;

            AddDomainEvent(new UserReactivatedEvent(
                Id.Value,
                UpdatedAt.Value
            ));
        }
    }

    public class UserId : ValueObject
    {
        public Guid Value { get; private set; }

        private UserId(Guid value)
        {
            Value = value;
        }

        public static UserId NewId() => new UserId(Guid.NewGuid());
        public static UserId FromGuid(Guid value) => new UserId(value);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
