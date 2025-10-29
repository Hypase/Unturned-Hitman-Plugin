using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace HitmanPlugin
{
    public class StatsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "hitmanstats";
        public string Help => "View your hitman statistics";
        public string Syntax => "";
        public List<string> Aliases => new List<string> { "hstats", "hitstats" };
        public List<string> Permissions => new List<string> { "hitman.stats" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            string stats = HitmanPlugin.Instance.GetHitManager().GetStats(player);
            UnturnedChat.Say(player, stats);
        }
    }
