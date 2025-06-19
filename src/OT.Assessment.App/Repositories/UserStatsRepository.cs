using Dapper;
using OT.Assessment.App.Data;
using OT.Assessment.App.Models;
using System.Data;

namespace OT.Assessment.App.Repositories
{
    public class UserStatsRepository : IUserStatsRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public UserStatsRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<TopSpenderDto>> GetTopSpendersAsync(int count)
        {
            using var connection = _connectionFactory.CreateConnection();

            var topSpenders = await connection.QueryAsync<TopSpenderDto>(
                "GetTopSpenders",
                new { Count = count },
                commandType: CommandType.StoredProcedure);

            return topSpenders.ToList();
        }
    }
}
