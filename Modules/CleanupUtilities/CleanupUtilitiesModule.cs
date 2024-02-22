using System.Collections.Generic;
using System.Linq;
using MEC;
using UniverseModule.API.Item;
using UniverseModule.API.Map;
using UniverseModule.Events;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.CleanupUtilities;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
public class CleanupUtilitiesModule
{
    private const float CleanupInterval = 180f;

    private readonly ItemManager _itemManager;

    private readonly MapManager _mapManager;

    private CoroutineHandle _automaticCleanup;

    public CleanupUtilitiesModule(ItemManager itemManager, MapManager mapManager, MapEvents mapEvents, RoundEvents roundEvents)
    {
        _itemManager = itemManager;
        _mapManager = mapManager;
        
        mapEvents.DetonateWarhead.Subscribe(OnDetonateWarhead);
        mapEvents.AnnounceDecontamination.Subscribe(OnAnnounceDecontamination);
        roundEvents.Start.Subscribe(OnStart);
        roundEvents.End.Subscribe(OnEnd);
    }

    private void OnDetonateWarhead(DetonateWarheadEvent ev) => ManagedCleanup(true);

    private void OnAnnounceDecontamination(AnnounceDecontaminationEvent ev)
    {
        if (ev.NextPhase is not 6)
            return;
        
        ManagedCleanup();
    }
    
    private void OnStart(RoundStartEvent _) => _automaticCleanup = Timing.RunCoroutine(AutomaticCleanup());
    
    private void OnEnd(RoundEndEvent _) => Timing.KillCoroutines(_automaticCleanup);

    private void ManagedCleanup(bool warhead = false)
    {
        if (warhead)
        {
            foreach (var item in _itemManager.AllItems)
            {
                if (item.Position.y > 800)
                    continue;
            
                item.Destroy();
            }
        
            foreach (var ragdoll in _mapManager.UniverseRagdolls)
            {
                if (ragdoll.Position.y > 800)
                    continue;
            
                ragdoll.Destroy();
            }
        }
        else
        {
            foreach (var item in _itemManager.AllItems)
            {
                if (item.Position.y is < 15f and > -1f)
                    item.Destroy();
            }
            
            foreach (var ragdoll in _mapManager.UniverseRagdolls)
            {
                if (ragdoll.Position.y is < 15f and > -1f)
                    ragdoll.Destroy();
            }
        }
    }

    private IEnumerator<float> AutomaticCleanup()
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(CleanupInterval);
            
            var items = GetItemsInGround();

            if (items.Count >= 1)
                continue;
            
            foreach (var item in items)
                item.Destroy();

            var ragDolls = _mapManager.UniverseRagdolls.ToList();
            
            if (ragDolls.Count >= 1)
                continue;
            
            foreach (var ragdoll in ragDolls)
                ragdoll.Destroy();
        }
        
        // ReSharper disable IteratorNeverReturns
    }
    
    private List<UniverseItem> GetItemsInGround()
    {
        var universeItems = _itemManager.AllItems.Where(item => item.Position.y < 0).ToList();
        return universeItems;
    }
}