using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace HitmanPlugin
{
    public class HitmanPlugin : RocketPlugin<HitmanConfiguration>
    {
        public static HitmanPlugin Instance;
        private HitManager hitManager;
        private TranslationManager translationManager;

        protected override void Load()
        {
            Instance = this;
            hitManager = new HitManager();
            translationManager = new TranslationManager();

            // Add event handler
            gameObject.AddComponent<HitmanEventHandler>();

            Logger.Log("Hitman Plugin loaded successfully!");
            Logger.Log("Permissions: hitman.hit, hitman.hits, hitman.claim, hitman.stats");
        }

        protected override void Unload()
        {
            hitManager.SaveData();
            Logger.Log("Hitman Plugin unloaded!");
        }

        public HitManager GetHitManager() => hitManager;
        public TranslationManager GetTranslationManager() => translationManager;
    }

    public class HitmanConfiguration : IRocketPluginConfiguration
    {
        public float HitExpireTime { get; set; }
        public int BaseReward { get; set; }
        public int RewardPerLevel { get; set; }
        public int ExperiencePerKill { get; set; }
        public int ExperiencePerClaim { get; set; }
        public int MaxActiveHits { get; set; }
        public float HitCooldown { get; set; }
        public bool UseEconomy { get; set; }
        public bool BroadcastHits { get; set; }
        public bool BroadcastClaim { get; set; }
        public int MinReward { get; set; }
        public int MaxReward { get; set; }
        public bool RemoveHitOnTargetDeath { get; set; }

        public void LoadDefaults()
        {
            HitExpireTime = 300f;
            BaseReward = 500;
            RewardPerLevel = 50;
            ExperiencePerKill = 100;
            ExperiencePerClaim = 50;
            MaxActiveHits = 3;
            HitCooldown = 60f;
            UseEconomy = true;
            BroadcastHits = true;
            BroadcastClaim = true;
            MinReward = 100;
            MaxReward = 5000;
            RemoveHitOnTargetDeath = true;
        }
    }

    public class TranslationManager
    {
        private Dictionary<string, string> translations;

        public TranslationManager()
        {
            LoadTranslations();
        }

        private void LoadTranslations()
        {
            string path = Path.Combine(HitmanPlugin.Instance.Directory, "HitmanTranslations.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            else
            {
                CreateDefaultTranslations(path);
            }
        }

        private void CreateDefaultTranslations(string path)
        {
            translations = new Dictionary<string, string>
            {
                ["hit_created"] = "Hit created on {0} for ${1}. Expires in {2} seconds.",
                ["hit_claimed"] = "Hit on {0} claimed by {1} for ${2}.",
                ["hit_completed"] = "Hit completed on {0}! You earned ${1} and {2} experience.",
                ["hit_expired"] = "Hit on {0} has expired.",
                ["hit_list"] = "Active hits: {0}",
                ["hit_list_entry"] = "{0} - ${1} - {2}s remaining",
                ["no_active_hits"] = "No active hits.",
                ["no_permission"] = "You don't have permission to use this command.",
                ["player_not_found"] = "Player not found.",
                ["player_offline"] = "Target player is offline.",
                ["hit_exists"] = "There is already an active hit on this player.",
                ["hit_cooldown"] = "You must wait {0} seconds before creating another hit.",
                ["max_hits_reached"] = "Maximum number of active hits reached.",
                ["no_hit_found"] = "No active hit found on this player.",
                ["cannot_hit_yourself"] = "You cannot place a hit on yourself.",
                ["hitman_stats"] = "Hitman Stats - Level: {0}, Experience: {1}/{2}, Total Kills: {3}",
                ["economy_disabled"] = "Economy is disabled on this server."
            };

            string json = JsonConvert.SerializeObject(translations, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public string Translate(string key, params object[] args)
        {
            if (translations.TryGetValue(key, out string translation))
            {
                return args.Length > 0 ? string.Format(translation, args) : translation;
            }
            return $"[Missing translation: {key}]";
        }
    }

    public class HitData
    {
        public string TargetName { get; set; }
        public string TargetId { get; set; }
        public string CreatorName { get; set; }
        public string CreatorId { get; set; }
        public string ClaimantId { get; set; }
        public string ClaimantName { get; set; }
        public int Reward { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ExpireTime { get; set; }
        public bool IsClaimed { get; set; }
        public bool IsCompleted { get; set; }
    }

    public class HitmanData
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public int TotalKills { get; set; }
        public DateTime LastHitTime { get; set; }

        public int GetRequiredExperience()
        {
            return Level * 1000 + 500;
        }

        public void AddExperience(int amount)
        {
            Experience += amount;
            while (Experience >= GetRequiredExperience())
            {
                Experience -= GetRequiredExperience();
                Level++;
            }
        }
    }

    public class HitManager
    {
        private List<HitData> activeHits;
        private Dictionary<string, HitmanData> hitmanData;
        private string dataPath;

        public HitManager()
        {
            dataPath = Path.Combine(HitmanPlugin.Instance.Directory, "HitmanData.json");
            activeHits = new List<HitData>();
            hitmanData = new Dictionary<string, HitmanData>();
            LoadData();
        }

        public void LoadData()
        {
            if (File.Exists(dataPath))
            {
                string json = File.ReadAllText(dataPath);
                var data = JsonConvert.DeserializeObject<HitmanSaveData>(json);
                activeHits = data.ActiveHits ?? new List<HitData>();
                hitmanData = data.HitmanData ?? new Dictionary<string, HitmanData>();
            }
        }

        public void SaveData()
        {
            var saveData = new HitmanSaveData
            {
                ActiveHits = activeHits,
                HitmanData = hitmanData
            };

            string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
            File.WriteAllText(dataPath, json);
        }

        private HitmanData GetOrCreateHitmanData(string playerId, string playerName)
        {
            if (!hitmanData.TryGetValue(playerId, out HitmanData data))
            {
                data = new HitmanData
                {
                    PlayerId = playerId,
                    PlayerName = playerName,
                    Level = 1,
                    Experience = 0,
                    TotalKills = 0,
                    LastHitTime = DateTime.MinValue
                };
                hitmanData[playerId] = data;
            }
            else
            {
                data.PlayerName = playerName;
            }
            return data;
        }

        public bool CreateHit(UnturnedPlayer creator, UnturnedPlayer target, int reward, out string message)
        {
            var config = HitmanPlugin.Instance.Configuration.Instance;
            var translations = HitmanPlugin.Instance.GetTranslationManager();

            if (creator.Id == target.Id)
            {
                message = translations.Translate("cannot_hit_yourself");
                return false;
            }

            if (activeHits.Exists(h => h.TargetId == target.Id))
            {
                message = translations.Translate("hit_exists");
                return false;
            }

            var creatorData = GetOrCreateHitmanData(creator.Id, creator.DisplayName);

            if ((DateTime.Now - creatorData.LastHitTime).TotalSeconds < config.HitCooldown)
            {
                float remaining = config.HitCooldown - (float)(DateTime.Now - creatorData.LastHitTime).TotalSeconds;
                message = translations.Translate("hit_cooldown", Mathf.CeilToInt(remaining));
                return false;
            }

            if (activeHits.Count >= config.MaxActiveHits)
            {
                message = translations.Translate("max_hits_reached");
                return false;
            }

            if (reward < config.MinReward || reward > config.MaxReward)
            {
                message = $"Reward must be between ${config.MinReward} and ${config.MaxReward}.";
                return false;
            }

            var hit = new HitData
            {
                TargetName = target.DisplayName,
                TargetId = target.Id,
                CreatorName = creator.DisplayName,
                CreatorId = creator.Id,
                Reward = reward,
                CreatedTime = DateTime.Now,
                ExpireTime = DateTime.Now.AddSeconds(config.HitExpireTime),
                IsClaimed = false,
                IsCompleted = false
            };

            activeHits.Add(hit);
            creatorData.LastHitTime = DateTime.Now;

            message = translations.Translate("hit_created", target.DisplayName, reward, config.HitExpireTime);
            return true;
        }

        public void OnTargetDied(string targetId)
        {
            var config = HitmanPlugin.Instance.Configuration.Instance;
            var translations = HitmanPlugin.Instance.GetTranslationManager();

            if (config.RemoveHitOnTargetDeath)
            {
                var hit = activeHits.Find(h => h.TargetId == targetId && !h.IsCompleted);
                if (hit != null)
                {
                    activeHits.Remove(hit);
                    if (config.BroadcastHits)
                    {
                        UnturnedChat.Say(translations.Translate("hit_expired", hit.TargetName));
                    }
                }
            }
        }

        public bool ClaimHit(UnturnedPlayer claimant, string targetName, out string message)
        {
            var translations = HitmanPlugin.Instance.GetTranslationManager();
            var hit = activeHits.Find(h =>
                h.TargetName.Equals(targetName, StringComparison.OrdinalIgnoreCase) &&
                !h.IsClaimed &&
                h.ExpireTime > DateTime.Now);

            if (hit == null)
            {
                message = translations.Translate("no_hit_found");
                return false;
            }

            hit.ClaimantId = claimant.Id;
            hit.ClaimantName = claimant.DisplayName;
            hit.IsClaimed = true;

            var config = HitmanPlugin.Instance.Configuration.Instance;
            if (config.BroadcastClaim)
            {
                UnturnedChat.Say(translations.Translate("hit_claimed", hit.TargetName, claimant.DisplayName, hit.Reward));
            }
            else
            {
                message = translations.Translate("hit_claimed", hit.TargetName, claimant.DisplayName, hit.Reward);
            }

            message = translations.Translate("hit_claimed", hit.TargetName, claimant.DisplayName, hit.Reward);
            return true;
        }

        public void CompleteHit(string targetId, UnturnedPlayer killer)
        {
            var hit = activeHits.Find(h => h.TargetId == targetId && h.IsClaimed && !h.IsCompleted);
            if (hit != null && hit.ClaimantId == killer.Id)
            {
                var config = HitmanPlugin.Instance.Configuration.Instance;
                var translations = HitmanPlugin.Instance.GetTranslationManager();

                var claimantData = GetOrCreateHitmanData(killer.Id, killer.DisplayName);
                claimantData.TotalKills++;
                claimantData.AddExperience(config.ExperiencePerKill);

                if (config.UseEconomy)
                {
                    // Uconomy integration would go here
                }

                hit.IsCompleted = true;
                activeHits.Remove(hit);

                UnturnedChat.Say(killer, translations.Translate("hit_completed", hit.TargetName, hit.Reward, config.ExperiencePerKill));
            }
        }

        public string GetHitList()
        {
            var translations = HitmanPlugin.Instance.GetTranslationManager();

            if (activeHits.Count == 0)
                return translations.Translate("no_active_hits");

            var hitEntries = new List<string>();
            foreach (var hit in activeHits)
            {
                if (hit.ExpireTime > DateTime.Now && !hit.IsCompleted)
                {
                    float remaining = (float)(hit.ExpireTime - DateTime.Now).TotalSeconds;
                    hitEntries.Add(translations.Translate("hit_list_entry", hit.TargetName, hit.Reward, Mathf.CeilToInt(remaining)));
                }
            }

            if (hitEntries.Count == 0)
                return translations.Translate("no_active_hits");

            return translations.Translate("hit_list", string.Join(", ", hitEntries));
        }

        public string GetStats(UnturnedPlayer player)
        {
            var data = GetOrCreateHitmanData(player.Id, player.DisplayName);
            var translations = HitmanPlugin.Instance.GetTranslationManager();

            return translations.Translate("hitman_stats", data.Level, data.Experience, data.GetRequiredExperience(), data.TotalKills);
        }

        public void CheckExpiredHits()
        {
            for (int i = activeHits.Count - 1; i >= 0; i--)
            {
                if (activeHits[i].ExpireTime <= DateTime.Now && !activeHits[i].IsCompleted)
                {
                    activeHits.RemoveAt(i);
                }
            }
        }
    }

    public class HitmanSaveData
    {
        public List<HitData> ActiveHits { get; set; }
        public Dictionary<string, HitmanData> HitmanData { get; set; }
    }
}
