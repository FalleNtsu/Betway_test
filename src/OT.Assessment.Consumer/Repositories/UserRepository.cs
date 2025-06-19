using Dapper;
using OT.Assessment.Consumer.Data;
using System.Data;

namespace OT.Assessment.Consumer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public UserRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task CreateUserIfNotExistsAsync(Guid accountId, string username)
        {
            using var connection = _connectionFactory.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("accountId", accountId);
            parameters.Add("username", username);

            await connection.ExecuteAsync("CreateUser", parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
