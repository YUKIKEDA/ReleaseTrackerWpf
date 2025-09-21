using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.Entities.User.Aggregates;

namespace Infrastructure.Caching.Redis.Strategies.User
{
    public interface IUserCacheStrategy
    {
        Task<UserAggregate> GetUserByIdAsync(Guid userId);
        Task<UserAggregate> GetUserByEmailAsync(string email);
        Task SetUserAsync(UserAggregate user);
        Task RemoveUserAsync(Guid userId);
        Task RemoveUserByEmailAsync(string email);
        Task InvalidateUserCacheAsync(Guid userId);
        Task<bool> IsUserCachedAsync(Guid userId);
        Task<long> GetUserCacheExpiryAsync(Guid userId);
    }

    public class UserCacheStrategy : IUserCacheStrategy
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<UserCacheStrategy> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly TimeSpan _defaultExpiry;
        private readonly TimeSpan _extendedExpiry;

        private const string USER_BY_ID_KEY_PREFIX = "user:id:";
        private const string USER_BY_EMAIL_KEY_PREFIX = "user:email:";
        private const string USER_CACHE_METADATA_PREFIX = "user:cache:metadata:";

        public UserCacheStrategy(
            IDistributedCache cache,
            ILogger<UserCacheStrategy> logger)
        {
            _cache = cache;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            _defaultExpiry = TimeSpan.FromMinutes(30);
            _extendedExpiry = TimeSpan.FromHours(2);
        }

        public async Task<UserAggregate> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var cacheKey = $"{USER_BY_ID_KEY_PREFIX}{userId}";
                var cachedData = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogDebug("User {UserId} not found in cache", userId);
                    return null;
                }

                var userCacheData = JsonSerializer.Deserialize<UserCacheData>(cachedData, _jsonOptions);
                
                // Check if cache entry has expired
                if (userCacheData.ExpiresAt < DateTime.UtcNow)
                {
                    await RemoveUserAsync(userId);
                    _logger.LogDebug("User {UserId} cache entry expired", userId);
                    return null;
                }

                _logger.LogDebug("User {UserId} retrieved from cache", userId);
                return userCacheData.ToUserAggregate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId} from cache", userId);
                return null;
            }
        }

        public async Task<UserAggregate> GetUserByEmailAsync(string email)
        {
            try
            {
                var cacheKey = $"{USER_BY_EMAIL_KEY_PREFIX}{email.ToLowerInvariant()}";
                var userIdString = await _cache.GetStringAsync(cacheKey);

                if (string.IsNullOrEmpty(userIdString))
                {
                    _logger.LogDebug("User with email {Email} not found in cache", email);
                    return null;
                }

                if (!Guid.TryParse(userIdString, out var userId))
                {
                    _logger.LogWarning("Invalid user ID format in cache for email {Email}: {UserIdString}", email, userIdString);
                    await RemoveUserByEmailAsync(email);
                    return null;
                }

                return await GetUserByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email {Email} from cache", email);
                return null;
            }
        }

        public async Task SetUserAsync(UserAggregate user)
        {
            try
            {
                var userCacheData = UserCacheData.FromUserAggregate(user);
                var userJson = JsonSerializer.Serialize(userCacheData, _jsonOptions);
                var userId = user.Id.Value;

                // Determine cache expiry based on user activity
                var expiry = DetermineCacheExpiry(user);

                // Cache user by ID
                var userIdCacheKey = $"{USER_BY_ID_KEY_PREFIX}{userId}";
                var userIdOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry
                };
                await _cache.SetStringAsync(userIdCacheKey, userJson, userIdOptions);

                // Cache email to ID mapping
                var emailCacheKey = $"{USER_BY_EMAIL_KEY_PREFIX}{user.Email.ToLowerInvariant()}";
                var emailOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry
                };
                await _cache.SetStringAsync(emailCacheKey, userId.ToString(), emailOptions);

                // Cache metadata
                await SetCacheMetadataAsync(userId, expiry);

                _logger.LogDebug("User {UserId} cached with expiry {Expiry}", userId, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching user {UserId}", user.Id.Value);
                throw;
            }
        }

        public async Task RemoveUserAsync(Guid userId)
        {
            try
            {
                // Get user email before removing
                var userCacheKey = $"{USER_BY_ID_KEY_PREFIX}{userId}";
                var cachedData = await _cache.GetStringAsync(userCacheKey);
                
                if (!string.IsNullOrEmpty(cachedData))
                {
                    var userCacheData = JsonSerializer.Deserialize<UserCacheData>(cachedData, _jsonOptions);
                    
                    // Remove email mapping
                    var emailCacheKey = $"{USER_BY_EMAIL_KEY_PREFIX}{userCacheData.Email.ToLowerInvariant()}";
                    await _cache.RemoveAsync(emailCacheKey);
                }

                // Remove user data
                await _cache.RemoveAsync(userCacheKey);

                // Remove metadata
                await RemoveCacheMetadataAsync(userId);

                _logger.LogDebug("User {UserId} removed from cache", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from cache", userId);
            }
        }

        public async Task RemoveUserByEmailAsync(string email)
        {
            try
            {
                var emailCacheKey = $"{USER_BY_EMAIL_KEY_PREFIX}{email.ToLowerInvariant()}";
                var userIdString = await _cache.GetStringAsync(emailCacheKey);

                if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var userId))
                {
                    await RemoveUserAsync(userId);
                }
                else
                {
                    await _cache.RemoveAsync(emailCacheKey);
                }

                _logger.LogDebug("User with email {Email} removed from cache", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user by email {Email} from cache", email);
            }
        }

        public async Task InvalidateUserCacheAsync(Guid userId)
        {
            await RemoveUserAsync(userId);
            _logger.LogDebug("User {UserId} cache invalidated", userId);
        }

        public async Task<bool> IsUserCachedAsync(Guid userId)
        {
            try
            {
                var cacheKey = $"{USER_BY_ID_KEY_PREFIX}{userId}";
                var cachedData = await _cache.GetStringAsync(cacheKey);
                return !string.IsNullOrEmpty(cachedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} is cached", userId);
                return false;
            }
        }

        public async Task<long> GetUserCacheExpiryAsync(Guid userId)
        {
            try
            {
                var metadataKey = $"{USER_CACHE_METADATA_PREFIX}{userId}";
                var metadataJson = await _cache.GetStringAsync(metadataKey);

                if (string.IsNullOrEmpty(metadataJson))
                    return 0;

                var metadata = JsonSerializer.Deserialize<UserCacheMetadata>(metadataJson, _jsonOptions);
                var remainingTicks = (metadata.ExpiresAt - DateTime.UtcNow).Ticks;
                return Math.Max(0, remainingTicks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache expiry for user {UserId}", userId);
                return 0;
            }
        }

        private TimeSpan DetermineCacheExpiry(UserAggregate user)
        {
            // Active users get extended cache time
            if (user.IsActive && user.IsEmailVerified)
            {
                return _extendedExpiry;
            }

            // Inactive or unverified users get shorter cache time
            return _defaultExpiry;
        }

        private async Task SetCacheMetadataAsync(Guid userId, TimeSpan expiry)
        {
            var metadata = new UserCacheMetadata
            {
                UserId = userId,
                CachedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(expiry)
            };

            var metadataJson = JsonSerializer.Serialize(metadata, _jsonOptions);
            var metadataKey = $"{USER_CACHE_METADATA_PREFIX}{userId}";
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            };

            await _cache.SetStringAsync(metadataKey, metadataJson, options);
        }

        private async Task RemoveCacheMetadataAsync(Guid userId)
        {
            var metadataKey = $"{USER_CACHE_METADATA_PREFIX}{userId}";
            await _cache.RemoveAsync(metadataKey);
        }
    }

    public class UserCacheData
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime ExpiresAt { get; set; }

        public static UserCacheData FromUserAggregate(UserAggregate user)
        {
            return new UserCacheData
            {
                Id = user.Id.Value,
                Email = user.Email,
                FirstName = user._profile.FirstName,
                LastName = user._profile.LastName,
                DateOfBirth = user._profile.DateOfBirth,
                PhoneNumber = user._profile.PhoneNumber,
                Address = user._address.Street,
                City = user._address.City,
                Country = user._address.Country,
                PostalCode = user._address.PostalCode,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified,
                ExpiresAt = DateTime.UtcNow.AddHours(2) // Default expiry
            };
        }

        public UserAggregate ToUserAggregate()
        {
            // This is a simplified conversion - in a real implementation,
            // you would need to reconstruct the full aggregate with all its components
            throw new NotImplementedException("Full aggregate reconstruction from cache data requires domain factory");
        }
    }

    public class UserCacheMetadata
    {
        public Guid UserId { get; set; }
        public DateTime CachedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
