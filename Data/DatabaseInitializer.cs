using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingManagmint.Data
{
    public static class DatabaseInitializer
    {
        public static void EnsureCreatedAndSeed(IServiceProvider services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is not configured.");

            // Use simple SQL script to create tables and seed data.
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "docs", "sqlserver_init.sql");
            if (!File.Exists(scriptPath))
            {
                // Fallback: look relative to content root
                var altPath = Path.Combine(Directory.GetCurrentDirectory(), "docs", "sqlserver_init.sql");
                if (File.Exists(altPath)) scriptPath = altPath;
            }

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException("SQL init script not found at docs/sqlserver_init.sql");
            }

            var script = File.ReadAllText(scriptPath);
            using var conn = new SqlConnection(connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = script;
            cmd.ExecuteNonQuery();
        }
    }
}

