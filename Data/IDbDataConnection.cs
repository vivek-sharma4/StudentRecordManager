using System.Data.Common;

namespace StudentRecordManager.Data
{
    public interface IDbDataConnection
    {
        DbConnection CreateConnection();
    }
}
