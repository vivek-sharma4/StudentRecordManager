using StudentRecordManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudentRecordManager.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int userId);
        Task<int> CreateUserAsync(User user);
        Task<bool> UpdateUserAuthorizationAsync(int userId, bool isAuthorized);
        Task<IEnumerable<User>> GetPendingUsersAsync();
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<bool> DeleteUserAsync(int userId);
    }
}
