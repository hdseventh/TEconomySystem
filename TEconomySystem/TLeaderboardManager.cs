using System.Data;
using System.Text;

namespace TEconomySystem
{
    public static class TLeaderboardManager
    {
        public static async Task<List<LeaderboardEntry>> GetLeaderboardAsync()
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
}
