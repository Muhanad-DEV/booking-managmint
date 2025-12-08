using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BookingManagmint.Data
{
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        public SqlConnection CreateConnection() => new SqlConnection(_connectionString);
    }
}

