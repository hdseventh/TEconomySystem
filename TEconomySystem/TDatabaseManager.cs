using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;

namespace TEconomySystem
{
    public static class TDatabaseManager
    {
        private static MySqlConnection connection;
        static string connectionString = ConfigManager.ConfigData.ConnectionString;

        public static void Initialize()
        {
            connection = new MySqlConnection(connectionString);
            connection.Open();

            string createUserTableQuery = @"CREATE TABLE IF NOT EXISTS users (
                id INT AUTO_INCREMENT PRIMARY KEY,
                username VARCHAR(255) UNIQUE NOT NULL,
                balance DECIMAL(10, 2) NOT NULL DEFAULT 0.00
            )";
            ExecuteNonQuery(createUserTableQuery);

            string createTransactionTableQuery = @"CREATE TABLE IF NOT EXISTS transactions (
                id INT AUTO_INCREMENT PRIMARY KEY,
                user_id INT NOT NULL,
                type ENUM('deposit', 'withdrawal', 'transfer') NOT NULL,
                amount DECIMAL(10, 2) NOT NULL,
                target_user_id INT,
                timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (user_id) REFERENCES users(id),
                FOREIGN KEY (target_user_id) REFERENCES users(id)
            )";
            ExecuteNonQuery(createTransactionTableQuery);

            string createSystemAccountQuery = "INSERT IGNORE INTO users (username, balance) VALUES (system, 0.00)";
            ExecuteNonQuery(createSystemAccountQuery);
        }

        public static void Dispose()
        {
            if (connection != null)
            {
                connection.Close();
                connection = null;
            }
        }

        public static async Task<int> ExecuteNonQueryAsync(string query, params MySqlParameter[] parameters)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters);
                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        private static void ExecuteNonQuery(string query)
        {
            using (var command = new MySqlCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        public static async Task<DbDataReader> ExecuteQueryAsync(string query, params MySqlParameter[] parameters)
        {
            var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            var command = new MySqlCommand(query, connection);
            command.Parameters.AddRange(parameters);
            return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }
    }
}
