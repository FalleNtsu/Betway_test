namespace OT.Assessment.Consumer.Repositories
{
    public interface IUserRepository
    {
        Task CreateUserIfNotExistsAsync(Guid accountId, string username);
    }
}
