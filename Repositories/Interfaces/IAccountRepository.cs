using StudentRecordManager.Models;

namespace StudentRecordManager.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<User?> GetUserByUsernameAsync(string username);

        Task<int> CreateUserAsync(User user);
    }
}
