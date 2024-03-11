using BaphometPlugin.Modules.CustomHud;
using PlayerRoles;
using UniverseModule.API.Map;
using UniverseModule.Events;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.DisconnectReplacer;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
public class DisconnectReplacerManager
{
    private readonly RoundManager _roundManager;

    public DisconnectReplacerManager(RoundManager roundManager, PlayerEvents playerEvents)
    {
        _roundManager = roundManager;
        
        playerEvents.Leave.Subscribe(OnLeave);
    }

    private void OnLeave(LeaveEvent ev)
    {
        if (!_roundManager.RoundIsActive || _roundManager.RoundEnded)
            return;

        if (ev.Player.RoleType is RoleTypeId.Spectator or RoleTypeId.Overwatch or RoleTypeId.None or RoleTypeId.Tutorial)
            return;
        
        if (!PlayerReplacer.TryGetRandomSpectator(out var player))
            return;
        
        PlayerReplacer.ReplacePlayer(player, ev.Player);
        player.SendHudHint(ScreenZone.CompletelyBottom, "<b>R<lowercase>eplaced a player that was disconnected</lowercase></b>", 5f);
    }
}