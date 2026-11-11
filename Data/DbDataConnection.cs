using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace StudentRecordManager.Data
{
    public class DbDataConnection : IDbDataConnection
    {
        private readonly IConfiguration _configuration;

        public DbDataConnection(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbConnection CreateConnection()
        {
            var connectionString = _configuration.GetConnectionString("StudentConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("StudentConnection is missing from appsettings.json.");
            }

            return new SqlConnection(connectionString);
        }
    }
}
