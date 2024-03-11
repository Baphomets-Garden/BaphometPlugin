using UniverseModule.API.Item;
using UniverseModule.Enums;
using UniverseModule.Events;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.CustomItems.Admin;

[AutomaticExecution]
[Item(Id = 201, Name = "Ban Gun", BasedItemType = ItemType.GunCOM15)]
public class BanGun(PlayerEvents player, ItemEvents item) : UniverseCustomItem(item, player)
{
    private readonly PlayerEvents _player1 = player;
    
    private const long BanTime = 60;

    private const string BanReason = "Trolled";
    
    public override void HookEvents()
    {
        _player1.Damage.Subscribe(OnDamage);
        _player1.DropItem.Subscribe(OnDropItem);
        base.HookEvents();
    }

    public override void UnHookEvents()
    {
        _player1.Damage.Unsubscribe(OnDamage);
        _player1.DropItem.Unsubscribe(OnDropItem);
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
