namespace Divar.Repositories
{
    public interface IAdvertisementRepository
    {
        Task<IEnumerable<Advertisement>> GetAllAdvertisementsAsync(int pageNumber, int pageSize, CategoryType? category = null, string searchTerm = "");
        Task<Advertisement> GetAdvertisementByIdAsync(int id);
        Task AddAdvertisementAsync(Advertisement advertisement);
        Task UpdateAdvertisementAsync(Advertisement advertisement);
        Task DeleteAdvertisementAsync(int id);
        Task<int> GetTotalAdvertisementsCountAsync(CategoryType? category = null, string searchTerm = "");
        Task<IEnumerable<Advertisement>> GetAdvertisementsByUserIdAsync(string userId, int pageNumber, int pageSize); // Changed int to string
        Task<int> GetTotalAdvertisementsCountByUserIdAsync(string userId); // Changed int to string
    }
}
