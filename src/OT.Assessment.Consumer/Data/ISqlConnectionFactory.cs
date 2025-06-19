using System.Data;

namespace OT.Assessment.Consumer.Data
{
    public interface ISqlConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
