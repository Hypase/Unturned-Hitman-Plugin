using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace HitmanPlugin
{
    public class HitmanEventHandler : MonoBehaviour
    {
        private void Start()
        {
            DamageTool.damagePlayerRequested += OnDamagePlayerRequested;
        }

        private void OnDestroy()
        {
            DamageTool.damagePlayerRequested -= OnDamagePlayerRequested;
        }

        private static void OnDamagePlayerRequested(ref DamagePlayerParameters parameters, ref bool shouldAllow)
        {
            if (parameters.player != null &&
                parameters.player.life.health <= parameters.damage)
            {
                UnturnedPlayer victim = UnturnedPlayer.FromPlayer(parameters.player);

                if (victim != null)
                {
                    HitmanPlugin.Instance.GetHitManager().OnTargetDied(victim.Id);

                    if (parameters.killer != CSteamID.Nil)
                    {
                        UnturnedPlayer killerPlayer = UnturnedPlayer.FromCSteamID(parameters.killer);
                        if (killerPlayer != null)
                        {
                            HitmanPlugin.Instance.GetHitManager().CompleteHit(victim.Id, killerPlayer);
                        }
                    }
                }
            }
        }
    }
}
