using System.Data;

namespace OT.Assessment.App.Data
{
    public interface ISqlConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
