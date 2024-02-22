using UniverseModule.API.Item;
using UniverseModule.Enums;
using UniverseModule.Events;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.CustomItems.Admin;

[AutomaticExecution]
[Item(Id = 201, Name = "Ban Gun", BasedItemType = ItemType.GunCOM15)]
public class BanGun(PlayerEvents player, ItemEvents item) : CustomItemHandler(item, player)
{
    private const long BanTime = 60;

    private const string BanReason = "Trolled";
    
    public override void HookEvents()
    {
        player.Damage.Subscribe(OnDamage);
        player.DropItem.Subscribe(OnDropItem);
        base.HookEvents();
    }

    public override void UnHookEvents()
    {
        player.Damage.Unsubscribe(OnDamage);
        player.DropItem.Unsubscribe(OnDropItem);
        base.UnHookEvents();
    }

    private void OnDamage(DamageEvent ev)
    {
        if (ev.Attacker?.Inventory.ItemInHand.Id is not 201) return;

        if (ev.Player.PlayerType is PlayerType.Dummy or PlayerType.Server) return;
        
        ev.Allow = false;
        ev.Player.Ban(BanTime, BanReason);
        ev.Attacker.Inventory.ItemInHand.Destroy();
    }

    private void OnDropItem(DropItemEvent ev)
    {
        if (ev.ItemToDrop.Id is not 201) return;
        
        ev.Allow = false;
    }
}