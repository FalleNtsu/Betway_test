using Dapper;
using OT.Assessment.App.Data;
using System.Data;

namespace OT.Assessment.App.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        public AuthRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<bool> IsValidUserAsync(Guid apiKey, string password)
        {
            using var connection = _connectionFactory.CreateConnection();
            var result = await connection.QuerySingleAsync<int>(
                "dbo.ValidateAccess",
                new { APIKey = apiKey, Password = password },
                commandType: CommandType.StoredProcedure);

            return result == 1;
        }
    }
}
