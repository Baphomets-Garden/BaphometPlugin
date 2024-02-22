using UniverseModule.Events;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.AfkChecker;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
public class AfkHandler
{
    public AfkHandler(PlayerEvents playerEvents) => playerEvents.Join.Subscribe(OnJoin);

    private void OnJoin(JoinEvent ev) => AfkCheckerComponent.AddController(ev.Player);
}