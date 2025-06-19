namespace OT.Assessment.App.Repositories
{
    public interface IUserRepository
    {
        Task CreateUserIfNotExistsAsync(Guid accountId, string username);
    }
}
