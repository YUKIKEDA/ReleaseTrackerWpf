using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.User.Aggregates;
using Domain.Events.User.EventHandlers;
using Infrastructure.Persistence.Repositories.User.Implementations;

namespace Application.UseCases.UserManagement.Commands.Handlers
{
    public class CreateUserCommand : IRequest<CreateUserResult>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
    }

    public class CreateUserResult
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsEmailVerified { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
    {
        private readonly IUserAggregateRepository _userRepository;
        private readonly IUserDomainEventPublisher _eventPublisher;
        private readonly IUserValidationService _validationService;
        private readonly IUserNotificationService _notificationService;

        public CreateUserCommandHandler(
            IUserAggregateRepository userRepository,
            IUserDomainEventPublisher eventPublisher,
            IUserValidationService validationService,
            IUserNotificationService notificationService)
        {
            _userRepository = userRepository;
            _eventPublisher = eventPublisher;
            _validationService = validationService;
            _notificationService = notificationService;
        }

        public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Validate business rules
            await _validationService.ValidateEmailUniquenessAsync(request.Email);
            await _validationService.ValidatePasswordStrengthAsync(request.Password);
            await _validationService.ValidateUserDataAsync(request);

            // Create user aggregate
            var userAggregate = UserAggregate.Create(
                firstName: request.FirstName,
                lastName: request.LastName,
                email: request.Email,
                password: request.Password,
                dateOfBirth: request.DateOfBirth,
                phoneNumber: request.PhoneNumber,
                address: new UserAddress(
                    street: request.Address,
                    city: request.City,
                    country: request.Country,
                    postalCode: request.PostalCode
                )
            );

            // Save to repository
            await _userRepository.SaveAsync(userAggregate);

            // Publish domain events
            await _eventPublisher.PublishAsync(userAggregate.GetUncommittedEvents());

            // Send welcome notification
            await _notificationService.SendWelcomeEmailAsync(userAggregate.Email, userAggregate.FullName);

            // Return result
            return new CreateUserResult
            {
                UserId = userAggregate.Id,
                Email = userAggregate.Email,
                FullName = userAggregate.FullName,
                CreatedAt = userAggregate.CreatedAt,
                IsEmailVerified = userAggregate.IsEmailVerified
            };
        }
    }
}
