﻿using MySql.Data.MySqlClient;
using System.Text;
using System.Data;
using TShockAPI;
using MonoMod;
using System.Collections.Generic;

namespace TEconomySystem
{
    public static class TCommands
    {
        public static async void Balance(CommandArgs args)
        {
            var player = args.Player;
            var balance = await GetBalanceAsync(player.Name);
            player.SendInfoMessage($"Your current balance is: {balance} Boks");
        }

        public static async void Deposit(CommandArgs args)
        {
            var player = args.Player;
            if (args.Parameters.Count < 2 || !decimal.TryParse(args.Parameters[1], out decimal amount) || amount <= 0)
            {
                player.SendErrorMessage("Invalid amount.");
                return;
            }

            string targetUser = args.Parameters.Count > 2 ? args.Parameters[2] : player.Name;
            if (!await UserExistsAsync(targetUser))
            {
                player.SendErrorMessage($"User '{targetUser}' does not exist.");
                return;
            }

            await AdjustBalanceAsync(targetUser, amount);
            await LogTransactionAsync(targetUser, "deposit", amount, targetUser);
            player.SendSuccessMessage($"Deposited {amount} Boks to {targetUser}'s account.");
        }

        public static async void Withdraw(CommandArgs args)
        {
            var player = args.Player;
            if (args.Parameters.Count < 2 || !decimal.TryParse(args.Parameters[1], out decimal amount) || amount <= 0)
            {
                player.SendErrorMessage("Invalid amount.");
                return;
            }

            var balance = await GetBalanceAsync(player.Name);
            if (balance < amount)
            {
                player.SendErrorMessage("Insufficient funds.");
                return;
            }

            string targetUser = args.Parameters.Count > 2 ? args.Parameters[2] : player.Name;
            if (!await UserExistsAsync(targetUser))
            {
                player.SendErrorMessage($"User '{targetUser}' does not exist.");
                return;
            }

            await AdjustBalanceAsync(targetUser, -amount);
            await LogTransactionAsync(player.Name, "withdrawal", amount, targetUser);
            player.SendSuccessMessage($"Withdrew {amount} Boks from {targetUser}'s account.");
        }

        public static async void Transfer(CommandArgs args)
        {
            var player = args.Player;
            if (args.Parameters.Count < 3 || !decimal.TryParse(args.Parameters[2], out decimal amount) || amount <= 0)
            {
                player.SendErrorMessage("Invalid amount or target user.");
                return;
            }

            string targetUser = args.Parameters[1];
            if (!await UserExistsAsync(targetUser))
            {
                player.SendErrorMessage($"User '{targetUser}' does not exist.");
                return;
            }

            var balance = await GetBalanceAsync(player.Name);
            if (balance < amount)
            {
                player.SendErrorMessage("Insufficient funds.");
                return;
            }

            await AdjustBalanceAsync(player.Name, -amount);
            await AdjustBalanceAsync(targetUser, amount);
            await LogTransactionAsync(player.Name, "transfer", amount, targetUser);
            player.SendSuccessMessage($"Transferred {amount} Boks to {targetUser}.");
        }

        public static async void Leaderboard(CommandArgs args)
        {
            var player = args.Player;
            var leaderboard = await GetLeaderboardAsync();
            int rank = 1;
            player.SendInfoMessage("Leaderboard:");
            foreach (var entry in leaderboard)
            {
                if(rank == 10 ) break;
                player.SendInfoMessage($"{entry.Username}: {entry.Balance} Boks");
                rank++;
            }
        }

        private static async Task<decimal> GetBalanceAsync(string username)
        {
            string query = "SELECT balance FROM users WHERE username = @username";
            var parameter = new MySqlParameter("@username", MySqlDbType.VarChar) { Value = username };
            using (var reader = await TDatabaseManager.ExecuteQueryAsync(query, parameter))
            {
                if (await reader.ReadAsync())
                {
                    return reader.GetDecimal("balance");
                }
                else
                {
                    await CreateUserAsync(username);
                    return 0;
                }
            }
        }

        private static async Task AdjustBalanceAsync(string username, decimal amount)
        {
            string query = "UPDATE users SET balance = balance + @amount WHERE username = @username";
            var parameters = new[]
            {
                new MySqlParameter("@amount", MySqlDbType.Decimal) { Value = amount },
                new MySqlParameter("@username", MySqlDbType.VarChar) { Value = username }
            };
            await TDatabaseManager.ExecuteNonQueryAsync(query, parameters);
        }

        private static async Task LogTransactionAsync(string username, string type, decimal amount, string targetUser = null)
        {
            string query = "INSERT INTO transactions (user_id, type, amount, target_user_id) VALUES ((SELECT id FROM users WHERE username = @username), @type, @amount, (SELECT id FROM users WHERE username = @targetUser))";
            var parameters = new[]
            {
                new MySqlParameter("@username", MySqlDbType.VarChar) { Value = username },
                new MySqlParameter("@type", MySqlDbType.Enum) { Value = type },
                new MySqlParameter("@amount", MySqlDbType.Decimal) { Value = amount },
                new MySqlParameter("@targetUser", MySqlDbType.VarChar) { Value = targetUser }
            };
            await TDatabaseManager.ExecuteNonQueryAsync(query, parameters);
        }

        private static async Task CreateUserAsync(string username)
        {
            string query = "INSERT INTO users (username) VALUES (@username)";
            var parameter = new MySqlParameter("@username", MySqlDbType.VarChar) { Value = username };
            await TDatabaseManager.ExecuteNonQueryAsync(query, parameter);
        }

        private static async Task<bool> UserExistsAsync(string username)
        {
            string query = "SELECT EXISTS(SELECT 1 FROM users WHERE username = @username LIMIT 1)";
            var parameter = new MySqlParameter("@username", MySqlDbType.VarChar) { Value = username };
            using (var reader = await TDatabaseManager.ExecuteQueryAsync(query, parameter))
            {
                if (await reader.ReadAsync())
                {
                    return reader.GetBoolean(0);
                }
                return false;
            }
        }

        private static async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
        {
            string query = "SELECT username, balance FROM users ORDER BY balance DESC LIMIT 10";
            var leaderboard = new List<LeaderboardEntry>();
            using (var reader = await TDatabaseManager.ExecuteQueryAsync(query))
            {
                while (await reader.ReadAsync())
                {
                    leaderboard.Add(new LeaderboardEntry
                    {
                        Username = reader.GetString("username"),
                        Balance = reader.GetDecimal("balance")
                    });
                }
            }
            return leaderboard;
        }
    }

    public class LeaderboardEntry
    {
        public string Username { get; set; }
        public decimal Balance { get; set; }
    }
}
