using UniverseModule.Events;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.CustomHud;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
public class HudHandler
{
    public HudHandler(PlayerEvents playerEvents, RoundEvents roundEvents)
    {
        playerEvents.Join.Subscribe(OnJoin);
        roundEvents.Restart.Subscribe(OnRestart);
        playerEvents.Leave.Subscribe(OnLeave);
    }

    private void OnJoin(JoinEvent ev) => ev.Player.StartCustomManager();

    private void OnRestart(RoundRestartEvent ev)
    {
        PluginExtensions.ClearHubs();
        PluginExtensions.ClearCustomManagers();
    }
    
    private void OnLeave(LeaveEvent ev) => ev.Player.RemoveCustomManager();
}