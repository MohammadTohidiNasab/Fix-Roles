namespace Divar.Repositories
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task AddUserAsync(CustomUser user);
        Task<CustomUser?> GetUserByEmailAsync(string email);
    }
}