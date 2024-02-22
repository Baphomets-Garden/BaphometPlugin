using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaphometPlugin.Modules.CustomHud;
using GameCore;
using MEC;
using PlayerRoles;
using PlayerRoles.Voice;
using UnityEngine;
using UniverseModule.API.Map;
using UniverseModule.API.Map.Rooms;
using UniverseModule.API.Player;
using UniverseModule.API.Server;
using UniverseModule.Events;
using UniverseModule.Generic.Core;
using VoiceChat;
using VoiceChat.Networking;

namespace BaphometPlugin.Modules.Lobby;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
public class LobbyManager
{
    private readonly PlayerManager _playerManager;
    
    private readonly ServerManager _serverManager;
    
    private readonly RoundManager _roundManager;
    
    public LobbyManager(PlayerManager playerManager, ServerManager serverManager, RoundManager roundManager, RoundEvents roundEvents, PlayerEvents playerEvents)
    {
        _playerManager = playerManager;
        _serverManager = serverManager;
        _roundManager = roundManager;
        
        roundEvents.Waiting.Subscribe(OnWaiting);
        roundEvents.Start.Subscribe(OnStart);
        playerEvents.Join.Subscribe(OnJoin);
        playerEvents.Speak.Subscribe(OnSpeak);
        playerEvents.DoorInteract.Subscribe(OnDoorInteract);
        playerEvents.CallVanillaElevator.Subscribe(OnCallElevator);
    }

    private void OnStart(RoundStartEvent ev)
    {
        foreach (var player in _playerManager.Players)
            player.GodMode = false;

        Timing.KillCoroutines("GameCoreCheck");
        Timing.KillCoroutines("LobbyMessages");

    }

    private void OnWaiting(RoundWaitingEvent ev)
    {
        GameObject.Find("StartRound").transform.localScale = Vector3.zero;
        Timing.RunCoroutine(GameCoreCheck(), "GameCoreCheck");
        Timing.RunCoroutine(LobbyMessages(), "LobbyMessages");
    }

    private void OnSpeak(SpeakEvent ev)
    {
        if (_roundManager.RoundIsActive || _roundManager.RoundEnded) return;
        
        ev.Allow = false;
        SendGlobalVoice(ev.VoiceMessage);
        return;

        void SendGlobalVoice(VoiceMessage msg)
        {
            msg.Channel = VoiceChatChannel.RoundSummary;
            foreach (ReferenceHub referenceHub in ReferenceHub.AllHubs)
            {
                if (referenceHub.connectionToClient == null || referenceHub.roleManager.CurrentRole is not IVoiceRole voiceRole2)
                    continue;
            
                if (voiceRole2.VoiceModule.ValidateReceive(msg.Speaker, VoiceChatChannel.Proximity) == VoiceChatChannel.None)
                    continue;
                    
                referenceHub.connectionToClient.Send(msg);
            }
        }
    }
    
    private void OnJoin(JoinEvent ev)
    {
        if (!_roundManager.RoundIsActive && (RoundStart.singleton.NetworkTimer > 1 ||
                                             RoundStart.singleton.NetworkTimer == -2))
        {
            Timing.CallDelayed(1.0f, () =>
            {
                if (_roundManager.RoundIsActive || (RoundStart.singleton.NetworkTimer <= 1 &&
                                                    RoundStart.singleton.NetworkTimer != -2)) return;

                ev.Player.OverWatch = false;
                ev.Player.SetRoleFlags(RoleTypeId.Tutorial, RoleSpawnFlags.All, RoleChangeReason.RemoteAdmin);
            });
            Timing.CallDelayed(1.5f, () =>
            {
                if (_roundManager.RoundIsActive || (RoundStart.singleton.NetworkTimer <= 1 &&
                                                    RoundStart.singleton.NetworkTimer != -2)) return;
                ev.Player.GodMode = true;
                
                ev.Player.Position = Universe.GetManagedClass<UniverseRoomManager>().Rooms.First(x => x.Id == (uint)RoomType.Scp106).GameObject.transform.TransformPoint(21.66187f, 1.571289f, -9.633276f);
                ev.Player.RotationHorizontal += 90;

                ev.Player.Inventory.ClearAllItems();
            });
        }
    }
    
    private void OnDoorInteract(DoorInteractEvent ev)
    {
        if (!_roundManager.RoundIsActive && !_roundManager.RoundEnded)
            ev.Allow = false;
    }
    
    private void OnCallElevator(CallVanillaElevatorEvent ev)
    {
        if (!_roundManager.RoundIsActive && !_roundManager.RoundEnded)
            ev.Allow = false;
    }
    
    private IEnumerator<float> GameCoreCheck()
    {
        // ReSharper disable IteratorNeverReturns
        
        while (true)
        {
            if (RoundStart.singleton.NetworkTimer == 0)
                foreach (var player in _playerManager.Players.Where(x => !x.OverWatch))
                    player.SetRoleFlags(RoleTypeId.None, RoleSpawnFlags.All);
            
            if (_playerManager.PlayersAmount == _serverManager.Slots)
                foreach (var player in _playerManager.Players.Where(x => !x.OverWatch))
                    player.SetRoleFlags(RoleTypeId.None, RoleSpawnFlags.All);

            yield return Timing.WaitForSeconds(1f);
        }
    }

    private IEnumerator<float> LobbyMessages()
    {
        while (true)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("<size=80%><b><color=#b069ca>W<lowercase>elcome to</lowercase></color> <color=yellow>⭐</color> <color=#b069ca>B<lowercase>aphomet's</lowercase> G<lowercase>arden</lowercase></color> <color=yellow>⭐</color></b></size>\n");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("<size=75%><b><color=#5cc8f2>discord.gg/baphomet</color></b></size>\n");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"<size=95%><b><color=#7086fa>{GetStatus(RoundStart.singleton.NetworkTimer)}</color></b></size>");
            
            foreach (var pl in _playerManager.Players)
                pl.SendHudHint(ScreenZone.Top, stringBuilder.ToString(), 1.2f);
            
            yield return Timing.WaitForSeconds(1f);
        }
    }

    private string GetStatus(short timer)
    {
        switch (timer)
        {
            case -2:
                return "<b><color=red>⛔</color> round paused <color=red>⛔</color></b>";
            case -1:
            case 0:
                return "<b><color=#5cc8f2>starting round</color></b>";
            default:
                return $"<b>{timer}s</b>\n<b><color=#5cc8f2>{_playerManager.PlayersAmount}/{_serverManager.Slots} players</color></b>";
        }
    }
}