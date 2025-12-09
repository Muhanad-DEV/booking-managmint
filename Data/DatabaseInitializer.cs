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

            var scriptPath = Path.Combine(AppContext.BaseDirectory, "docs", "sqlserver_init.sql");
            if (!File.Exists(scriptPath))
            {
                var altPath = Path.Combine(Directory.GetCurrentDirectory(), "docs", "sqlserver_init.sql");
                if (File.Exists(altPath)) scriptPath = altPath;
            }

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException("SQL init script not found at docs/sqlserver_init.sql");
            }

            var script = File.ReadAllText(scriptPath);
            
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog ?? "BookingManagmint";
            builder.InitialCatalog = "master";
            var masterConnectionString = builder.ConnectionString;
            
            var maxRetries = 30;
            var delay = TimeSpan.FromSeconds(2);
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    using (var masterConn = new SqlConnection(masterConnectionString))
                    {
                        masterConn.Open();
                        using var createDbCmd = masterConn.CreateCommand();
                        createDbCmd.CommandText = $@"
                            IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '{databaseName}')
                            BEGIN
                                CREATE DATABASE [{databaseName}];
                            END";
                        createDbCmd.ExecuteNonQuery();
                    }
                    
                    using var conn = new SqlConnection(connectionString);
                    conn.Open();
                    
                    var lines = script.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                    var currentBatch = new System.Text.StringBuilder();
                    
                    foreach (var line in lines)
                    {
                        var trimmed = line.Trim();
                        if (trimmed.Equals("GO", StringComparison.OrdinalIgnoreCase))
                        {
                            if (currentBatch.Length > 0)
                            {
                                var batchText = currentBatch.ToString().Trim();
                                if (!string.IsNullOrWhiteSpace(batchText))
                                {
                                    using var cmd = conn.CreateCommand();
                                    cmd.CommandTimeout = 60;
                                    cmd.CommandText = batchText;
                                    cmd.ExecuteNonQuery();
                                }
                                currentBatch.Clear();
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("--", StringComparison.OrdinalIgnoreCase))
                        {
                            currentBatch.AppendLine(line);
                        }
                    }
                    
                    if (currentBatch.Length > 0)
                    {
                        var batchText = currentBatch.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(batchText))
                        {
                            using var cmd = conn.CreateCommand();
                            cmd.CommandTimeout = 60;
                            cmd.CommandText = batchText;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    return;
                }
                catch (SqlException ex) when (i < maxRetries - 1)
                {
                    if (ex.Number == 4060 || ex.Number == 18456 || ex.Message.Contains("network-related") || ex.Message.Contains("Cannot open database"))
                    {
                        Thread.Sleep(delay);
                        continue;
                    }
                    throw;
                }
            }
        }
    }
}

