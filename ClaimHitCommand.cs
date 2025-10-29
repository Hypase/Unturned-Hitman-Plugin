using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace HitmanPlugin
{
    public class ClaimHitCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "claimhit";
        public string Help => "Claim an active hit";
        public string Syntax => "<player>";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "hitman.claim" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 1)
            {
                UnturnedChat.Say(caller, "Correct syntax: /claimhit <player>");
                return;
            }

            UnturnedPlayer player = (UnturnedPlayer)caller;
            string targetName = command[0];

            if (HitmanPlugin.Instance.GetHitManager().ClaimHit(player, targetName, out string message))
            {
                UnturnedChat.Say(player, message);
            }
            else
            {
                UnturnedChat.Say(player, message);
            }
        }
    }
}
