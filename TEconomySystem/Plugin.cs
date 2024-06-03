using TShockAPI;
using TerrariaApi.Server;
using Terraria;

namespace TEconomySystem
{
    [ApiVersion(2, 1)]
    public class TEconomySystem : TerrariaPlugin
    {
        public override string Name => "TEconomySystem";
        public override string Author => "hdseventh";
        public override string Description => "A simple economy system for tshock";
        public override Version Version => new Version(0, 1, 0);

        public TEconomySystem(Main game) : base(game)
        {
            Order = 1;
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("economy.use", EconomyMain, "boks", "eco"));

            TDatabaseManager.Initialize();
        }

        public static void EconomyMain(CommandArgs args)
        {
            if(args.Parameters.Count < 1)
            {
                if (args.Player.HasPermission("economy.manage"))
                {
                    args.Player.SendErrorMessage("Invalid command. Available commands: balance, deposit, withdraw, transfer, leaderboard");
                    return;
                }
                args.Player.SendErrorMessage("Invalid command. Available commands: balance, transfer, leaderboard");
                return;
            }

            switch (args.Parameters[0])
            {
                case "balance":
                case "bal":
                    TCommands.Balance(args);
                    break;
                case "deposit":
                case "dep":
                case "give":
                    if (args.Player.HasPermission("economy.manage"))
                    {
                        TCommands.Deposit(args);
                        break;
                    }
                    args.Player.SendErrorMessage("You do not have permission to use this command.");
                    break;
                case "reduce":
                case "take":
                    if (args.Player.HasPermission("economy.manage"))
                    {
                        TCommands.Withdraw(args);
                        break;
                    }
                    args.Player.SendErrorMessage("You do not have permission to use this command.");
                    break;
                case "transfer":
                case "tf":
                    TCommands.Transfer(args);
                    break;
                case "leaderboard":
                case "lb":
                    TCommands.Leaderboard(args);
                    break;
                default:
                    if (args.Player.HasPermission("economy.manage"))
                    {
                        args.Player.SendErrorMessage("Invalid command. Available commands: balance, deposit, withdraw, transfer, leaderboard");
                        break;
                    }
                    args.Player.SendErrorMessage("Invalid command. Available commands: balance, transfer, leaderboard");
                    break;

            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TDatabaseManager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}