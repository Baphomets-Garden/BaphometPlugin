using System.Collections.Generic;
using BaphometPlugin.Modules.CustomHud;
using PlayerRoles;
using PlayerRoles.Spectating;
using PlayerRoles.Voice;
using UnityEngine;
using UniverseModule.API.Player;
using UniverseModule.Events;
using UniverseModule.Generic.Core;
using VoiceChat;
using VoiceChat.Networking;

namespace BaphometPlugin.Modules.ScpVoiceChat;

// ReSharper disable ClassNeverInstantiated.Global

[AutomaticExecution]
public class ScpVoiceChatModule
{
    public ScpVoiceChatModule(PlayerEvents playerEvents, RoundEvents roundEvents)
    {
        playerEvents.Speak.Subscribe(OnSpeak);
        playerEvents.ToggleNoclip.Subscribe(OnToggleNoClip);
        roundEvents.Restart.Subscribe(OnRestart);
    }

    private readonly HashSet<UniversePlayer> _toggledPlayers = [];
    
    private readonly HashSet<RoleTypeId> _availableRoles =
    [
        RoleTypeId.Scp049,
        RoleTypeId.Scp0492,
        RoleTypeId.Scp096,
        RoleTypeId.Scp106,
        RoleTypeId.Scp173,
        RoleTypeId.Scp939
    ];
    
    private void OnRestart(RoundRestartEvent _) => _toggledPlayers.Clear();

    private void OnToggleNoClip(ToggleNoclipEvent ev)
    {
        if (ev.Player.NoClipPermitted)
            return;
        
        if (!_availableRoles.Contains(ev.Player.RoleType))
            return;
        
        ev.Allow = false;
        
        if (!_toggledPlayers.Add(ev.Player))
        {
            _toggledPlayers.Remove(ev.Player);
            ev.Player.SendHudHint(ScreenZone.CenterTop, "<b>P<lowercase>roximity chat <color=red>disabled</color></lowercase></b>", 4f);
            return;
        }

        ev.Player.SendHudHint(ScreenZone.CenterTop, "<b>P<lowercase>roximity chat <color=#42f57b>enabled</color></lowercase></b>", 4f);
    }
    
    private void OnSpeak(SpeakEvent ev)
    {
        if (ev.VoiceMessage.Channel != VoiceChatChannel.ScpChat)
            return;
        
        if (!_availableRoles.Contains(ev.Player.RoleType) || !_toggledPlayers.Contains(ev.Player))
            return;

        ev.Allow = false;
        SendProximityMessage(ev.VoiceMessage);
    }
    
    private static void SendProximityMessage(VoiceMessage msg)
    {
        foreach (var referenceHub in ReferenceHub.AllHubs)
        {
            if (referenceHub.roleManager.CurrentRole is SpectatorRole && !msg.Speaker.IsSpectatedBy(referenceHub))
                continue;
                
            if (referenceHub.roleManager.CurrentRole is not IVoiceRole voiceRole2)
                continue;
            
            if (Vector3.Distance(msg.Speaker.transform.position, referenceHub.transform.position) >= 7)
                continue;

            if (voiceRole2.VoiceModule.ValidateReceive(msg.Speaker, VoiceChatChannel.Proximity) == VoiceChatChannel.None)
                continue;
            
            msg.Channel = VoiceChatChannel.Proximity;
            referenceHub.connectionToClient.Send(msg);
        }
    }
}