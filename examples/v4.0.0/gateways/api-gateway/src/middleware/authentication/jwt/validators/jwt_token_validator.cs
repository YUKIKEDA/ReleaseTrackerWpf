using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Gateways.ApiGateway.Middleware.Authentication.Jwt.Validators
{
    public interface IJwtTokenValidator
    {
        Task<JwtValidationResult> ValidateTokenAsync(string token);
        Task<JwtValidationResult> ValidateTokenAsync(string token, string audience);
        Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(string token);
        Task<bool> IsTokenExpiredAsync(string token);
        Task<DateTime> GetTokenExpiryAsync(string token);
    }

    public class JwtTokenValidator : IJwtTokenValidator
    {
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly TokenValidationParameters _validationParameters;
        private readonly ILogger<JwtTokenValidator> _logger;

        public JwtTokenValidator(
            JwtSecurityTokenHandler tokenHandler,
            TokenValidationParameters validationParameters,
            ILogger<JwtTokenValidator> logger)
        {
            _tokenHandler = tokenHandler;
            _validationParameters = validationParameters;
            _logger = logger;
        }

        public async Task<JwtValidationResult> ValidateTokenAsync(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return new JwtValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Token is null or empty",
                        ErrorCode = JwtValidationErrorCode.InvalidToken
                    };
                }

                // Remove Bearer prefix if present
                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = token.Substring(7);
                }

                var principal = _tokenHandler.ValidateToken(token, _validationParameters, out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken)
                {
                    return new JwtValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Token is not a valid JWT",
                        ErrorCode = JwtValidationErrorCode.InvalidToken
                    };
                }

                // Additional custom validations
                var customValidationResult = await PerformCustomValidationsAsync(jwtToken, principal);
                if (!customValidationResult.IsValid)
                {
                    return customValidationResult;
                }

                return new JwtValidationResult
                {
                    IsValid = true,
                    Principal = principal,
                    Token = jwtToken,
                    UserId = GetUserIdFromClaims(principal),
                    Email = GetEmailFromClaims(principal),
                    Roles = GetRolesFromClaims(principal),
                    ExpiryTime = jwtToken.ValidTo
                };
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, "JWT token has expired");
                return new JwtValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Token has expired",
                    ErrorCode = JwtValidationErrorCode.TokenExpired
                };
            }
            catch (SecurityTokenNotYetValidException ex)
            {
                _logger.LogWarning(ex, "JWT token is not yet valid");
                return new JwtValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Token is not yet valid",
                    ErrorCode = JwtValidationErrorCode.TokenNotYetValid
                };
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning(ex, "JWT token has invalid signature");
                return new JwtValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Token has invalid signature",
                    ErrorCode = JwtValidationErrorCode.InvalidSignature
                };
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "JWT token validation failed");
                return new JwtValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Token validation failed",
                    ErrorCode = JwtValidationErrorCode.ValidationFailed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during JWT token validation");
                return new JwtValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Internal validation error",
                    ErrorCode = JwtValidationErrorCode.InternalError
                };
            }
        }

        public async Task<JwtValidationResult> ValidateTokenAsync(string token, string audience)
        {
            // Create a copy of validation parameters with specific audience
            var audienceValidationParameters = _validationParameters.Clone();
            audienceValidationParameters.ValidAudience = audience;

            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return new JwtValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Token is null or empty",
                        ErrorCode = JwtValidationErrorCode.InvalidToken
                    };
                }

                // Remove Bearer prefix if present
                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = token.Substring(7);
                }

                var principal = _tokenHandler.ValidateToken(token, audienceValidationParameters, out SecurityToken validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken)
                {
                    return new JwtValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = "Token is not a valid JWT",
                        ErrorCode = JwtValidationErrorCode.InvalidToken
                    };
                }

                // Verify audience matches
                if (!jwtToken.Audiences.Contains(audience))
                {
                    return new JwtValidationResult
                    {
                        IsValid = false,
                        ErrorMessage = $"Token audience does not match expected audience: {audience}",
                        ErrorCode = JwtValidationErrorCode.InvalidAudience
                    };
                }

                return new JwtValidationResult
                {
                    IsValid = true,
                    Principal = principal,
                    Token = jwtToken,
                    UserId = GetUserIdFromClaims(principal),
                    Email = GetEmailFromClaims(principal),
                    Roles = GetRolesFromClaims(principal),
                    ExpiryTime = jwtToken.ValidTo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating JWT token with audience {Audience}", audience);
                return new JwtValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Token validation failed",
                    ErrorCode = JwtValidationErrorCode.ValidationFailed
                };
            }
        }

        public async Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(string token)
        {
            var validationResult = await ValidateTokenAsync(token);
            return validationResult.IsValid ? validationResult.Principal : null;
        }

        public async Task<bool> IsTokenExpiredAsync(string token)
        {
            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo < DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking token expiry");
                return true; // Assume expired if we can't determine
            }
        }

        public async Task<DateTime> GetTokenExpiryAsync(string token)
        {
            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token expiry");
                return DateTime.MinValue;
            }
        }

        private async Task<JwtValidationResult> PerformCustomValidationsAsync(JwtSecurityToken jwtToken, ClaimsPrincipal principal)
        {
            // Check if token is blacklisted (if implemented)
            if (await IsTokenBlacklistedAsync(jwtToken.Id))
            {
                return new JwtValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Token has been blacklisted",
                    ErrorCode = JwtValidationErrorCode.TokenBlacklisted
                };
            }

            // Check if user is still active (if user service is available)
            var userId = GetUserIdFromClaims(principal);
            if (!string.IsNullOrEmpty(userId) && !await IsUserActiveAsync(userId))
            {
                return new JwtValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "User account is inactive",
                    ErrorCode = JwtValidationErrorCode.UserInactive
                };
            }

            // Check if token was issued before password change (if implemented)
            var passwordChangedAt = GetPasswordChangedAtFromClaims(principal);
            if (passwordChangedAt.HasValue && jwtToken.IssuedAt < passwordChangedAt.Value)
            {
                return new JwtValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Token was issued before password change",
                    ErrorCode = JwtValidationErrorCode.TokenSuperseded
                };
            }

            return new JwtValidationResult { IsValid = true };
        }

        private string GetUserIdFromClaims(ClaimsPrincipal principal)
        {
            return principal.FindFirst("sub")?.Value ?? 
                   principal.FindFirst("user_id")?.Value ?? 
                   principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private string GetEmailFromClaims(ClaimsPrincipal principal)
        {
            return principal.FindFirst("email")?.Value ?? 
                   principal.FindFirst(ClaimTypes.Email)?.Value;
        }

        private string[] GetRolesFromClaims(ClaimsPrincipal principal)
        {
            return principal.FindAll("role")
                .Select(c => c.Value)
                .Concat(principal.FindAll(ClaimTypes.Role).Select(c => c.Value))
                .ToArray();
        }

        private DateTime? GetPasswordChangedAtFromClaims(ClaimsPrincipal principal)
        {
            var passwordChangedAtClaim = principal.FindFirst("password_changed_at")?.Value;
            if (DateTime.TryParse(passwordChangedAtClaim, out var passwordChangedAt))
            {
                return passwordChangedAt;
            }
            return null;
        }

        private async Task<bool> IsTokenBlacklistedAsync(string tokenId)
        {
            // Implementation would check against a blacklist store (Redis, database, etc.)
            await Task.CompletedTask;
            return false;
        }

        private async Task<bool> IsUserActiveAsync(string userId)
        {
            // Implementation would check user service or cache
            await Task.CompletedTask;
            return true;
        }
    }

    public class JwtValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public JwtValidationErrorCode ErrorCode { get; set; }
        public ClaimsPrincipal Principal { get; set; }
        public JwtSecurityToken Token { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

    public enum JwtValidationErrorCode
    {
        None,
        InvalidToken,
        TokenExpired,
        TokenNotYetValid,
        InvalidSignature,
        InvalidAudience,
        TokenBlacklisted,
        UserInactive,
        TokenSuperseded,
        ValidationFailed,
        InternalError
    }
}
