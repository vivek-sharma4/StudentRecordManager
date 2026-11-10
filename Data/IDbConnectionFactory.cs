using System.Data.Common;

namespace StudentRecordManager.Data
{
    public interface IDbConnectionFactory
    {
        DbConnection CreateConnection();
    }
}
