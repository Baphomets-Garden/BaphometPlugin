using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BaphometPlugin.Modules.CustomHud;
using PlayerRoles;
using UnityEngine;
using UniverseModule.API.Player;
using UniverseModule.API.Role;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.CustomItems.Scp427;

[AutomaticExecution]
[Role(Id = 200, Name = "SCP-427-1", TeamId = (uint)Team.SCPs)]
public class Scp427Role : UniverseRole
{
    public override void SpawnPlayer(IUniverseRole previousRole = null, bool spawnLite = false)
    {
        Player.SendHudHint(ScreenZone.Center, RoleDescription, 6f);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        SetupRole(Player, CancellationToken.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    public override void DeSpawn(DeSpawnReason reason)
    {
        CancellationToken.Cancel();
        
        Player.Scale = Vector3.one;
        Player.MaxHealth = 100;
        Player.NicknameSync.Network_customPlayerInfoString = string.Empty;
        
        base.DeSpawn(reason);
    }

    public override string RoleDescription => "<b><color=#F00000>Y<lowercase>ou are</lowercase> SCP<lowercase>-427-1</lowercase></color></b>\n\n<b><color=#F00000>Y<lowercase>ou held SCP-427 \"Lovecraftian Locket\" for too long!\nAnd have become an abomination...</color></b>";

    public override List<uint> GetEnemiesID()
    {
        return
        [
            (uint)Team.ClassD,
            (uint)Team.Scientists,
            (uint)Team.FoundationForces,
            (uint)Team.ChaosInsurgency
        ];
    }
    
    public override List<uint> GetFriendsID()
    {
        return
        [
            (uint)Team.SCPs
        ];
    }
    
    private static readonly CancellationTokenSource CancellationToken = new();

    private static async Task SetupRole(UniversePlayer player, CancellationToken token)
    {
        await Task.Delay(2000, token);

        player.NicknameSync.Network_customPlayerInfoString = "<size=25><color=#C50000>SCP 427-1</color></size>";
        
        await Task.Delay(3000, token);

        player.Health = 700;
        player.MaxHealth = 700;
        player.Scale = new Vector3(1.2f, 1.2f, 1.2f);

        CancellationToken.Cancel();
    }
}