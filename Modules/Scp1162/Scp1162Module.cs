using System.Collections.Generic;
using System.Linq;
using BaphometPlugin.Modules.CustomHud;
using BaphometPlugin.Modules.Scp1162.Utils;
using UnityEngine;
using UniverseModule.API.Item;
using UniverseModule.API.Map.Rooms;
using UniverseModule.API.Player;
using UniverseModule.Enums;
using UniverseModule.Events;
using UniverseModule.Generic.Core;

namespace BaphometPlugin.Modules.Scp1162;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
public class Scp1162Module
{
    private readonly UniverseRoomManager _roomManager;
    
    public Scp1162Module(UniverseRoomManager roomManager, RoundEvents roundEvents, PlayerEvents playerEvents)
    {
        _roomManager = roomManager;
        
        roundEvents.Start.Subscribe(OnStart);
        playerEvents.DropItem.Subscribe(OnDropItem);
    }
    
    private Vector3 _scp1162Position;

    private void OnStart(RoundStartEvent _) => Spawn1162();

    private void OnDropItem(DropItemEvent ev)
    {
        if (Vector3.Distance(_scp1162Position, ev.Player.Position) <= 2f)
            OnUseScp1162(ev.Player, ev.ItemToDrop);
    }
    
    private void Spawn1162()
    {
        var room = _roomManager.Rooms.FirstOrDefault(x => x.GameObject.name == "LCZ_173");
        var scp1162 = new Scp1162Toy(PrimitiveType.Cylinder, new Vector3(17f, 13f, 3.59f), new Vector3(90f, 0f, 0f),
            new Vector3(1.0f, 0.1f, 1.0f), Color.black, room?.GameObject.transform, 0.95f).Spawn();

        _scp1162Position = scp1162.transform.position;
    }

    private static void OnUseScp1162(UniversePlayer player, UniverseItem itemToDrop)
    {
        var chance = Random.Range(0, 100);
        
        if (chance <= 35)
        {
            player.GiveEffect(Effect.SeveredHands);
            return;
        }
        
        var randomItem = Chances.RandomItem();
        
        player.Inventory.RemoveItem(itemToDrop);
        player.Inventory.GiveItem(randomItem);
        
        player.SendHudHint(ScreenZone.InteractionMessage, "<b>Y<lowercase>ou changed your item with the</lowercase> <color=#B92E34>SCP 1162</color> <lowercase>to obtain a new one</lowercase></b>", 6);
    }
    
    private static List<ItemType> Chances { get; } =
    [
        ItemType.Adrenaline,
        ItemType.Coin,
        ItemType.Flashlight,
        ItemType.GrenadeFlash,
        ItemType.GrenadeHE,
        ItemType.GunRevolver,
        ItemType.GunCOM15,
        ItemType.ArmorCombat,
        ItemType.ArmorHeavy,
        ItemType.ArmorLight,
        ItemType.KeycardChaosInsurgency,
        ItemType.KeycardScientist,
        ItemType.KeycardJanitor,
        ItemType.KeycardGuard,
        ItemType.Medkit,
        ItemType.Painkillers,
        ItemType.SCP018,
        ItemType.SCP500,
        ItemType.SCP1576,
        ItemType.SCP244a,
        ItemType.SCP244b,
        ItemType.SCP2176,
        ItemType.Radio,
        ItemType.KeycardZoneManager,
        ItemType.AntiSCP207
    ];
}