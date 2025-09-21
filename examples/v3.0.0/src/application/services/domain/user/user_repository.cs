using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Domain.User
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<bool> DeleteAsync(int id);
    }

    public class UserRepository : IUserRepository
    {
        private readonly IDbContext _dbContext;

        public UserRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            var query = "SELECT * FROM Users WHERE IsDeleted = 0";
            return await _dbContext.QueryAsync<User>(query);
        }

        public async Task<User> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM Users WHERE Id = @Id AND IsDeleted = 0";
            return await _dbContext.QueryFirstOrDefaultAsync<User>(query, new { Id = id });
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            var query = "SELECT * FROM Users WHERE Email = @Email AND IsDeleted = 0";
            return await _dbContext.QueryFirstOrDefaultAsync<User>(query, new { Email = email });
        }

        public async Task<User> CreateAsync(User user)
        {
            var query = @"
                INSERT INTO Users (Name, Email, CreatedAt, UpdatedAt, IsDeleted)
                VALUES (@Name, @Email, @CreatedAt, @UpdatedAt, @IsDeleted);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            var id = await _dbContext.ExecuteScalarAsync<int>(query, user);
            user.Id = id;
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            var query = @"
                UPDATE Users 
                SET Name = @Name, Email = @Email, UpdatedAt = @UpdatedAt
                WHERE Id = @Id AND IsDeleted = 0";

            await _dbContext.ExecuteAsync(query, user);
            return user;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var query = "UPDATE Users SET IsDeleted = 1, UpdatedAt = @UpdatedAt WHERE Id = @Id";
            var rowsAffected = await _dbContext.ExecuteAsync(query, new { Id = id, UpdatedAt = DateTime.UtcNow });
            return rowsAffected > 0;
        }
    }
}
