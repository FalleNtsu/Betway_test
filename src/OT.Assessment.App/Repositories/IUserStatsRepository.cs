using OT.Assessment.App.Models;

namespace OT.Assessment.App.Repositories
{
    public interface IUserStatsRepository
    {
        Task<List<TopSpenderDto>> GetTopSpendersAsync(int count);
    }
}
