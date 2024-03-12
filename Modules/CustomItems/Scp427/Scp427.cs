using System;
using System.Collections.Generic;
using BaphometPlugin.Modules.CustomHud;
using MEC;
using PlayerRoles;
using UniverseModule.API.Item;
using UniverseModule.API.Player;
using UniverseModule.Events;
using UniverseModule.Generic.Core;
using Random = UnityEngine.Random;

namespace BaphometPlugin.Modules.CustomItems.Scp427;

[AutomaticExecution]
[Item(Id = 203, Name = "SCP-427", BasedItemType = ItemType.Coin)]
public class Scp427(ItemEvents items, PlayerEvents player) : UniverseCustomItem(items, player)
{
    private const int MaxExposure = 50;
    
    private readonly PlayerEvents _player1 = player;
    
    private int _scp427Exposure;
    
    private bool _coroutinePaused;

    public override void HookEvents()
    {
        _player1.Pickup.Subscribe(OnPickupItem);
        _player1.DropItem.Subscribe(OnDropItem);
        _player1.ChangeItem.Subscribe(OnChangeItem);
        _player1.Death.Subscribe(OnDeath);
        base.HookEvents();
    }

    private void OnPickupItem(PickupEvent ev)
    {
        if (ev.Item.Id != 203) return;
        
        ev.Player.SendHudHint(ScreenZone.CenterBottom, "<b>Picked up <color=#F00000>SCP-427 \"Lovecraftian Locket\"</color>.</b>");
        _coroutinePaused = false;
    }

    private void OnDropItem(DropItemEvent ev)
    {
        if (ev.ItemToDrop.Id != 203) return;
        
        if (!_coroutinePaused)
            _coroutinePaused = true;
    }
    
    private void OnChangeItem(ChangeItemEvent ev)
    {
        if (ev.NewItem.Id is 203)
        {
            ev.Player.SendHudHint(ScreenZone.CenterBottom, "<b>This is <color=#F00000>SCP-427 \"Lovecraftian Locket\"</color> !\n<color=#F00000>WARNING:</color> Long exposure to this locket can result\nin the transformation of SCP-427-1! (Only works when held)</b>");
            Timing.RunCoroutine(Scp427Handler(ev.Player), $"{ev.Player.GenericUserId}-scp427");
            
            _coroutinePaused = false;
            
            return;
        }
        
        _coroutinePaused = true;
    }

    private void OnDeath(DeathEvent ev)
    {
        Timing.KillCoroutines($"{ev.Player.GenericUserId}-scp427");

        _coroutinePaused = false;
        
        _scp427Exposure = 0;
    }

    private IEnumerator<float> Scp427Handler(UniversePlayer player)
    {
        while (true)
        {
            if (_coroutinePaused)
            {
                yield return Timing.WaitForSeconds(1f);
                continue;
            }
            
            if (Math.Abs(player.Health - player.MaxHealth) > 0.1f)
                player.Health += Random.Range(5, 8);

            _scp427Exposure++;
            
            if (_scp427Exposure >= MaxExposure)
            {
                if (player.IsNearElevator)
                {
                    yield return Timing.WaitForSeconds(1f);
                    continue;
                }
                
                player.Inventory.DropEverything();
                var storedPosition = player.Position;

                yield return Timing.WaitForSeconds(0.2f);
                
                player.SetRoleFlags(RoleTypeId.Scp0492, RoleSpawnFlags.All);

                yield return Timing.WaitForSeconds(0.2f);
                
                player.RoleId = 200;

                yield return Timing.WaitForSeconds(0.4f);
                
                player.Position = storedPosition;
                
                _scp427Exposure = 0;
                
                yield break;
            }
            
            yield return Timing.WaitForSeconds(1f);
        }
    }
}