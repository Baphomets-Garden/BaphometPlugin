using UniverseModule.API.Item;
using UniverseModule.Enums;
using UniverseModule.Events;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.CustomItems;

[AutomaticExecution]
[Item(Id = 200, Name = "LunchBox", BasedItemType = ItemType.Medkit)]
public class LunchBox(ItemEvents items, PlayerEvents player) : UniverseCustomItem(items, player)
{
    public override void OnConsume(ConsumeItemEvent ev)
    {
        if (ev.State is ItemInteractState.Finalize)
        {
            ev.Player.Heal(100);
            ev.Player.GiveEffect(Effect.DamageReduction, 10);
        }
        
        base.OnConsume(ev);
    }
}
