namespace OT.Assessment.Consumer.Repositories
{
    public interface IUserStatsRepository
    {
        Task CreateOrUpdateUserStatsAsync(Guid accountId, decimal amountToAdd);
    }
}
