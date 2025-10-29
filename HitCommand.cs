using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace HitmanPlugin
{
    public class HitsCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "hits";
        public string Help => "View active hits";
        public string Syntax => "";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "hitman.hits" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            string hitList = HitmanPlugin.Instance.GetHitManager().GetHitList();
            UnturnedChat.Say(player, hitList);
        }
    }
}
