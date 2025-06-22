namespace OT.Assessment.App.Repositories
{
    public interface IAuthRepository
    {
        Task<bool> IsValidUserAsync(Guid apiKey, string password);
    }
}
