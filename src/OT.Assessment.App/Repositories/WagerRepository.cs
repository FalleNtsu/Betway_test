using Dapper;
using OT.Assessment.App.Models;
using OT.Assessment.App.Data;
using System.Data;

namespace OT.Assessment.App.Repositories
{
    public class WagerRepository : IWagerRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public WagerRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<(List<WagerDto> wagers, int total)> GetCasinoWagersForPlayerAsync(Guid accountId, int page, int pageSize)
        {
            using var connection = _connectionFactory.CreateConnection();
            var offset = (page - 1) * pageSize;

            using var multi = await connection.QueryMultipleAsync(
                "GetCasinoWagersForPlayer",
                new { AccountId = accountId, Offset = offset, PageSize = pageSize },
                commandType: CommandType.StoredProcedure);

            var total = await multi.ReadSingleAsync<int>();
            var wagers = (await multi.ReadAsync<WagerDto>()).ToList();

            return (wagers, total);
        }
    }
}
