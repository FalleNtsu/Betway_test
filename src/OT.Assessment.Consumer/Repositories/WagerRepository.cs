using Dapper;
using OT.Assessment.Consumer.Data;
using System.Data;

namespace OT.Assessment.Consumer.Repositories
{
    public class WagerRepository : IWagerRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public WagerRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task CreateWagerAsync(Guid wagerId, string game, string provider, decimal amount, DateTimeOffset createdDate, Guid transactionId, Guid accountId)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("wagerId", wagerId);
            parameters.Add("game", game);
            parameters.Add("provider", provider);
            parameters.Add("amount", amount);
            parameters.Add("createdDate", createdDate);
            parameters.Add("transactionId", transactionId);
            parameters.Add("accountId", accountId);

            await connection.ExecuteAsync("CreateWager", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
