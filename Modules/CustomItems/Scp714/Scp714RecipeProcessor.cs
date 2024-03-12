using System;
using Scp914;
using UnityEngine;
using UniverseModule.API.Item;
using UniverseModule.API.Map.Scp914;
using UniverseModule.Generic.Core;
using Random = UnityEngine.Random;

namespace BaphometPlugin.Modules.CustomItems.Scp714;

// ReSharper disable ObjectCreationAsStatement

[AutomaticExecution]
[Scp914Processor(ReplaceHandlers = [35])]
public class Scp714RecipeProcessor : IUniverse914Processor
{
    public void CreateUpgradedItem(UniverseItem item, Scp914KnobSetting setting, Vector3 position = default)
    {
        var chance = Random.Range(0, 100);

        if (setting != Scp914KnobSetting.Fine) return;
        
        if (chance <= 60) return;
        
        var state = item.State;
        var owner = item.ItemOwner;

        switch (state)
        {
            case ItemState.Map:
                new UniverseItem(202, position);
                break;

            case ItemState.Inventory:
                new UniverseItem(202, owner);
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
                throw new ArgumentOutOfRangeException();
        }
    }
}