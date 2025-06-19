using Microsoft.AspNetCore.Mvc;

namespace OT.Assessment.Consumer.Repositories
{
    public interface IWagerRepository
    {
        Task CreateWagerAsync(Guid wagerId, string game, string provider, decimal amount, DateTimeOffset createdDate, Guid transactionId, Guid accountId);
    }
}
