using System.Collections.Generic;
using CustomPlayerEffects;
using PlayerRoles;
using UniverseModule.API.Player;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.DisconnectReplacer;

public static class PlayerReplacer
{
    public static void ReplacePlayer(UniversePlayer replacer, UniversePlayer oldPlayer, bool checkCustomRole = true)
    {
        if (oldPlayer.GetEffect<PocketCorroding>().IsEnabled)
            return;
        
        oldPlayer.Inventory.DropEverything();
        var health = oldPlayer.Health;
        var maxHealth = oldPlayer.MaxHealth;
        var humeShield = oldPlayer.CurrentHumeShield;
            
        replacer.Position = oldPlayer.Position;
        replacer.SetRoleFlags(oldPlayer.RoleType, RoleSpawnFlags.None, RoleChangeReason.RemoteAdmin);
        replacer.Health = health;
        replacer.MaxHealth = maxHealth;
        
        if (checkCustomRole)
            replacer.RoleId = oldPlayer.RoleId;
        
        if (oldPlayer.Hub.IsSCP())
            replacer.CurrentHumeShield = humeShield;
    }
    
    public static bool TryGetRandomSpectator(out UniversePlayer player)
    {
        List<UniversePlayer> players = [];
        
        foreach (var possibleTarget in Universe.GetManagedClass<PlayerManager>().Players)
        {
            if (possibleTarget.RoleType != RoleTypeId.Spectator)
                continue;
            
            players.Add(possibleTarget);
        }

        if (players.Count == 0)
        {
            player = null;
            return false;
        }
        
        player = players.RandomItem();
        return true;
    }
}