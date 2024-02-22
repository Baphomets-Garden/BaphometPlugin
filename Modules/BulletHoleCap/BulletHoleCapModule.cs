using System.Collections.Generic;
using UniverseModule.API.Player;
using UniverseModule.Events;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.BulletHoleCap;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
public class BulletHoleCapModule
{
    public BulletHoleCapModule(PlayerEvents playerEvents, RoundEvents roundEvents)
    {
        playerEvents.PlaceBulletHole.Subscribe(OnPlaceBulletHole);
        roundEvents.Restart.Subscribe(OnRestart);
    }

    private static readonly Dictionary<UniversePlayer, byte> BulletHoleCounter = new();
    
    private void OnRestart(RoundRestartEvent _) => BulletHoleCounter.Clear();

    private void OnPlaceBulletHole(PlaceBulletHoleEvent ev)
    {
        if (BulletHoleCounter.TryGetValue(ev.Player, out var count))
        {
            if (count >= 75)
            {
                ev.Allow = false;
                return;
            }
            
            BulletHoleCounter[ev.Player]++;
            return;
        }
        
        BulletHoleCounter.Add(ev.Player, 1);
    }
}