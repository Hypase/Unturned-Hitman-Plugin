# 7k's Hitman Plugin 

A bounty system for Unturned servers where players can place, claim, and complete hits for rewards and experience.

## Features
- Experience-based leveling system
- Configurable min/max bounty amounts
- Hit expiration and cooldowns
- Automatic hit removal on target death
- Multi-language support
- Data persistence between restarts

## Commands & Permissions
| Command | Syntax | Permission |
|---------|--------|------------|
| `/hit` | `<player> <amount>` | `hitman.hit` |
| `/hits` | - | `hitman.hits` |
| `/claimhit` | `<player>` | `hitman.claim` |
| `/hitmanstats` | - | `hitman.stats` |

**Permission Setup:**
```xml
<Group Name="default">
  <Permission>hitman.hits</Permission>
  <Permission>hitman.stats</Permission>
</Group>
<Group Name="hitman">
  <Permission>hitman.hit</Permission>
  <Permission>hitman.claim</Permission>
</Group>
```

## How to Use

1. **Place a bounty:** `/hit PlayerName 1000`
2. **View active hits:** `/hits` 
3. **Claim a hit:** `/claimhit PlayerName`
4. **Complete the hit:** Eliminate the target
5. **Check stats:** `/hitmanstats`

## Configuration
Generated `HitmanConfig.json`:
```json
{
  "HitExpireTime": 300.0,
  "MinReward": 100,
  "MaxReward": 5000,
  "ExperiencePerKill": 100,
  "MaxActiveHits": 3,
  "HitCooldown": 60.0,
  "RemoveHitOnTargetDeath": true
}
```

## Installation
1. Drop `HitmanPlugin.dll` into `Rocket/Plugins/`
2. Configure permissions
3. Restart server
4. Customize config files as needed

**Requirements:** RocketMod 4.x, Unturned 3.x, .NET Framework 4.6.1+
