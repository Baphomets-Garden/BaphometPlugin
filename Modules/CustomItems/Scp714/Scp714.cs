using BaphometPlugin.Modules.CustomHud;
using PlayerRoles;
using UniverseModule.API.Item;
using UniverseModule.Enums;
using UniverseModule.Events;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.CustomItems.Scp714;

[AutomaticExecution]
[Item(Id = 202, Name = "SCP-714", BasedItemType = ItemType.Coin)]
public class Scp714(ItemEvents items, PlayerEvents player) : UniverseCustomItem(items, player)
{
    private readonly PlayerEvents _player1 = player;

    public override void HookEvents()
    {
        _player1.Pickup.Subscribe(OnPickupItem);
        _player1.ChangeItem.Subscribe(OnChangeItem);
        _player1.Damage.Subscribe(OnDamage);
        base.HookEvents();
    }

    public override void UnHookEvents()
    {
        _player1.Pickup.Unsubscribe(OnPickupItem);
        _player1.ChangeItem.Unsubscribe(OnChangeItem);
        _player1.Damage.Unsubscribe(OnDamage);
        base.UnHookEvents();
    }

    private void OnPickupItem(PickupEvent ev)
    {
        if (ev.Item.Id != 202) return;
        
        ev.Player.SendHudHint(ScreenZone.CenterBottom, "<b>Picked up <color=#F00000>SCP-714 \"The Jade Ring\"</color> !\nYou can survive 1 attack from SCP-049 (Only works when held)!</b>", 6f);
    }
    
    private void OnChangeItem(ChangeItemEvent ev)
    {
        if (ev.NewItem.Id != 202) return;

        ev.Player.SendHudHint(ScreenZone.CenterBottom, "<b>This is <color=#F00000>SCP-714 \"The Jade Ring\"</color> !\nYou can survive 1 attack from SCP-049 (Only works when held)!</b>", 6f);
    }

    private void OnDamage(DamageEvent ev)
    {
        if (ev.Attacker?.RoleType != RoleTypeId.Scp049) return;
        if (ev.Player.Inventory.ItemInHand.Id != 202) return;
        
        ev.Allow = false;
        ev.Player.Inventory.ItemInHand.Destroy();
        ev.Player.RemoveEffect(Effect.CardiacArrest);
        ev.Player.SendHudHint(ScreenZone.CenterBottom, "<b>You survived an attack from SCP-049 with <color=#F00000>SCP-714 \"The Jade Ring\"</color> !</b>", 6f);
        ev.Attacker.SendHudHint(ScreenZone.CenterBottom, "<b>Your target was wearing <color=#F00000>SCP-714 \"The Jade Ring\"</color> and damage was negated..</b>", 6f);
    }
}