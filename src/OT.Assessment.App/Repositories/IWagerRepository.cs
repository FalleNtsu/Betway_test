using Microsoft.AspNetCore.Mvc;
using OT.Assessment.App.Models;

namespace OT.Assessment.App.Repositories
{
    public interface IWagerRepository
    {
        Task<(List<WagerDto> wagers, int total)> GetCasinoWagersForPlayerAsync(Guid accountId, int page, int pageSize);
    }
}
