using Dapper;
using OT.Assessment.Consumer.Data;
using System.Data;

namespace OT.Assessment.Consumer.Repositories
{
    public class UserStatsRepository : IUserStatsRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public UserStatsRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task CreateOrUpdateUserStatsAsync(Guid accountId, decimal amountToAdd)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("accountId", accountId);
            parameters.Add("amountToAdd", amountToAdd);

            await connection.ExecuteAsync("CreateOrUpdateUserStat", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
