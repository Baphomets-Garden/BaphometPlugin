using System;
using Scp914;
using UnityEngine;
using UniverseModule.API.Item;
using UniverseModule.API.Map.Scp914;
using UniverseModule.Generic.Core;
using Random = UnityEngine.Random;

namespace BaphometPlugin.Modules.CustomItems.Scp427;

// ReSharper disable ObjectCreationAsStatement

[AutomaticExecution]
[Scp914Processor(ReplaceHandlers = [35])]
public class Scp427RecipeProcessor : IUniverse914Processor
{
    public void CreateUpgradedItem(UniverseItem item, Scp914KnobSetting setting, Vector3 position = new())
    {
        var chance = Random.Range(0, 100);

        if (setting != Scp914KnobSetting.VeryFine) return;
        
        if (chance <= 60) return;
        
        var state = item.State;
        var owner = item.ItemOwner;

        switch (state)
        {
            case ItemState.Map:
                new UniverseItem(203, position);
                break;

            case ItemState.Inventory:
                new UniverseItem(203, owner);
                break;
            
            case ItemState.BeforeSpawn:
                break;
            case ItemState.Thrown:
                break;
            case ItemState.Despawned:
                break;
            case ItemState.ServerSideOnly:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), "Invalid item state.");
        }
    }
}